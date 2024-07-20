using DevIO.Business.Configurations;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public static class ValidationMessagesBuilder
{
    public static ValidationMessages Build()
    {
        var validationMessages = new ValidationMessages
        {
            EmptyFileMessage = "EmptyFileMessage",
            FileAlreadyExistMessage = "FileAlreadyExistMessage",
            IncorrectUserOrPasswordMessage = "IncorrectUserOrPasswordMessage",
            LockedOutMessage = "LockedOutMessage",
            QueryError = "QueryError",
            SupplierAlreadyExist = "SupplierAlreadyExist",
            SupplierHasRegisteredProducts = "SupplierHasRegisteredProducts",
            SupplierNotFound = "SupplierNotFound",
        };

        return validationMessages;
    }
}