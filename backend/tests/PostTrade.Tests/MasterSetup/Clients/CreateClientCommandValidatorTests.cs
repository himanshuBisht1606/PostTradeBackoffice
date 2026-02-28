using PostTrade.Application.Features.MasterSetup.Clients.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Clients;

public class CreateClientCommandValidatorTests
{
    private readonly CreateClientCommandValidator _validator = new();

    private static CreateClientCommand ValidCommand() => new(
        BrokerId:       Guid.NewGuid(),
        BranchId:       null,
        ClientCode:     "CLT001",
        ClientName:     "John Doe",
        Email:          "john@example.com",
        Phone:          "9876543210",
        ClientType:     ClientType.Individual,
        PAN:            "ABCDE1234F",
        Aadhaar:        null,
        DPId:           null,
        DematAccountNo: null,
        Depository:     null,
        Address:        "123 Main St",
        StateCode:      null,
        StateName:      null,
        BankAccountNo:  "123456789",
        BankName:       "HDFC Bank",
        BankIFSC:       "HDFC0001234"
    );

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenBrokerIdIsEmpty_ShouldFail()
    {
        var command = ValidCommand() with { BrokerId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BrokerId");
    }

    [Fact]
    public void Validate_WhenClientCodeIsEmpty_ShouldFail()
    {
        var command = ValidCommand() with { ClientCode = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClientCode");
    }

    [Fact]
    public void Validate_WhenClientCodeExceedsMaxLength_ShouldFail()
    {
        var command = ValidCommand() with { ClientCode = new string('A', 21) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClientCode");
    }

    [Fact]
    public void Validate_WhenClientNameIsEmpty_ShouldFail()
    {
        var command = ValidCommand() with { ClientName = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClientName");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void Validate_WhenEmailIsInvalid_ShouldFail(string badEmail)
    {
        var command = ValidCommand() with { Email = badEmail };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenPhoneIsEmpty_ShouldFail()
    {
        var command = ValidCommand() with { Phone = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public void Validate_WhenOptionalFieldsAreNull_ShouldPass()
    {
        var command = ValidCommand() with
        {
            PAN           = null,
            Address       = null,
            BankAccountNo = null,
            BankName      = null,
            BankIFSC      = null
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
