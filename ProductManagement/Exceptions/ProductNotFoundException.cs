namespace ProductManagement.Exceptions;

public class ProductNotFoundException : BaseException
{
    protected ProductNotFoundException(Guid userId)
        : base($"Product with ID '{userId}' was not found.", 404, "PRODUCT_NOT_FOUND")
    {
    }
}