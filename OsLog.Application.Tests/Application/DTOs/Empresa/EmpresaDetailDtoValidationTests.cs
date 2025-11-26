using OsLog.Application.DTOs.Empresa;
using System.ComponentModel.DataAnnotations;

namespace OsLog.Tests.Application.DTOs.Empresa
{
    public class EmpresaDetailDtoValidationTests
    {
        // Helper centralizado para validar DataAnnotations
        private static IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        private static EmpresaDetailDto CriarValido()
        {
            return new EmpresaDetailDto
            {
                Id = 1,
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart Centro",
                Cnpj = "12345678000199",
                DataCriacao = DateTime.UtcNow,
                Ativa = true
            };
        }

        [Fact(DisplayName = "[DTO] EmpresaDetailDto válido não deve gerar erros")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        public void DetailDto_Valido_Nao_Deve_Gerar_Erros()
        {
            // Arrange
            var dto = CriarValido();

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Empty(results);
        }

        [Fact(DisplayName = "[DTO] EmpresaDetailDto sem RazaoSocial deve falhar em Required")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        public void DetailDto_Sem_RazaoSocial_Deve_Falhar_Required()
        {
            // Arrange
            var dto = CriarValido();
            dto.RazaoSocial = string.Empty;

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r => r.MemberNames != null &&
                                          r.MemberNames.Contains(nameof(EmpresaDetailDto.RazaoSocial)));
        }

        [Fact(DisplayName = "[DTO] EmpresaDetailDto com RazaoSocial > 150 deve falhar em StringLength")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        public void DetailDto_Com_RazaoSocial_Muito_Longa_Deve_Falhar_StringLength()
        {
            // Arrange
            var dto = CriarValido();
            dto.RazaoSocial = new string('R', 151); // 151 caracteres

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r => r.MemberNames != null &&
                                          r.MemberNames.Contains(nameof(EmpresaDetailDto.RazaoSocial)));
        }

        [Fact(DisplayName = "[DTO] EmpresaDetailDto sem NomeFantasia deve falhar em Required")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        public void DetailDto_Sem_NomeFantasia_Deve_Falhar_Required()
        {
            // Arrange
            var dto = CriarValido();
            dto.NomeFantasia = string.Empty;

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r => r.MemberNames != null &&
                                          r.MemberNames.Contains(nameof(EmpresaDetailDto.NomeFantasia)));
        }

        [Fact(DisplayName = "[DTO] EmpresaDetailDto com NomeFantasia > 120 deve falhar em StringLength")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        public void DetailDto_Com_NomeFantasia_Muito_Longo_Deve_Falhar_StringLength()
        {
            // Arrange
            var dto = CriarValido();
            dto.NomeFantasia = new string('N', 121); // 121 caracteres

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r => r.MemberNames != null &&
                                          r.MemberNames.Contains(nameof(EmpresaDetailDto.NomeFantasia)));
        }

        [Fact(DisplayName = "[DTO] EmpresaDetailDto sem Cnpj deve falhar em Required")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        public void DetailDto_Sem_Cnpj_Deve_Falhar_Required()
        {
            // Arrange
            var dto = CriarValido();
            dto.Cnpj = null;

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r => r.MemberNames != null &&
                                          r.MemberNames.Contains(nameof(EmpresaDetailDto.Cnpj)));
        }

        [Theory(DisplayName = "[DTO] EmpresaDetailDto com Cnpj inválido deve falhar em Regex")]
        [Trait("Category", "DTO.Validation")]
        [Trait("SubCategory", "EmpresaDetailDto")]
        [InlineData("123")]                 // curto demais
        [InlineData("123456789012345")]     // longo demais
        [InlineData("12A45678000199")]      // contém letra
        public void DetailDto_Com_Cnpj_Invalido_Deve_Falhar_Regex(string cnpjInvalido)
        {
            // Arrange
            var dto = CriarValido();
            dto.Cnpj = cnpjInvalido;

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r => r.MemberNames != null &&
                                          r.MemberNames.Contains(nameof(EmpresaDetailDto.Cnpj)));
        }
    }
}
