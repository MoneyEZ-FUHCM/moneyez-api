namespace MoneyEz.Repositories.Enums
{
    public enum GroupAction
    {
        // for group
        CREATED,
        UPDATED,
        DISBANDED,

        // for group member
        INVITED,
        JOINED,
        LEFT,
        KICKED,

        // for transaction group
        TRANSACTION_CREATED,
        TRANSACTION_UPDATED,
        TRANSACTION_DELETED,
    }
    public enum GroupStatus
    {
        ACTIVE,
        DISBANDED
    }
}