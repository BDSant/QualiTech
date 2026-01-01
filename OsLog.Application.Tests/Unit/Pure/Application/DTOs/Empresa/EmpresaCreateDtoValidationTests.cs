using OsLog.Application.DTOs.Empresa;
using System.ComponentModel.DataAnnotations;

namespace OsLog.Tests.Unit.Pure.Application.DTOs.Empresa
{
    public class EmpresaCreateDtoValidationTests
    {
        private static IList<ValidationResult> Validate(EmpresaCreateDto dto)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
            return results;
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve ser válido quando todos os campos estiverem corretos")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Ser_Valido_Quando_Todos_Os_Campos_Corretos()
        {
            // Arrange
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12345678000199" // 14 dígitos
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Empty(results);
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve falhar quando RazaoSocial não for informada")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Falhar_Quando_RazaoSocial_Obrigatoria()
        {
            // Arrange
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "", // vazio
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12345678000199"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames != null &&
                r.MemberNames.Contains(nameof(EmpresaCreateDto.RazaoSocial)) &&
                r.ErrorMessage == "Razão social é obrigatória.");
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve falhar quando RazaoSocial ultrapassar o tamanho máximo")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Falhar_Quando_RazaoSocial_Maior_Que_Maximo()
        {
            // Arrange
            var razaoMuitoGrande = new string('A', 151); // > 150
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = razaoMuitoGrande,
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12345678000199"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames != null &&
                r.MemberNames.Contains(nameof(EmpresaCreateDto.RazaoSocial)) &&
                r.ErrorMessage == "Razão social pode ter no máximo 150 caracteres.");
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve falhar quando NomeFantasia não for informado")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Falhar_Quando_NomeFantasia_Obrigatorio()
        {
            // Arrange
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "", // vazio
                Cnpj = "12345678000199"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames != null &&
                r.MemberNames.Contains(nameof(EmpresaCreateDto.NomeFantasia)) &&
                r.ErrorMessage == "Nome fantasia é obrigatório.");
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve falhar quando NomeFantasia ultrapassar o tamanho máximo")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Falhar_Quando_NomeFantasia_Maior_Que_Maximo()
        {
            // Arrange
            var nomeMuitoGrande = new string('B', 121); // > 120
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = nomeMuitoGrande,
                Cnpj = "12345678000199"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames != null &&
                r.MemberNames.Contains(nameof(EmpresaCreateDto.NomeFantasia)) &&
                r.ErrorMessage == "Nome fantasia pode ter no máximo 120 caracteres.");
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve falhar quando Cnpj não for informado")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Falhar_Quando_Cnpj_Obrigatorio()
        {
            // Arrange
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = null
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames != null &&
                r.MemberNames.Contains(nameof(EmpresaCreateDto.Cnpj)) &&
                r.ErrorMessage == "CNPJ é obrigatório.");
        }

        [Fact(DisplayName = "[DTO] EmpresaCreateDto deve falhar quando Cnpj não tiver 14 dígitos numéricos")]
        [Trait("Category", "DTO")]
        [Trait("SubCategory", "EmpresaCreateDto")]
        public void Deve_Falhar_Quando_Cnpj_Nao_Tiver_14_Digitos()
        {
            // Arrange
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12345678"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames != null &&
                r.MemberNames.Contains(nameof(EmpresaCreateDto.Cnpj)) &&
                r.ErrorMessage == "CNPJ deve conter 14 dígitos numéricos (apenas números).");
        }
    }
}
