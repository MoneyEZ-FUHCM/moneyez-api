using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Constants
{
    public class MessageConstants
    {
        // syntax error code: <CONSTANT_NAME> = "MessagePascalCaseCode";
        // syntax message: <CONSTANT_NAME_MESSAGE> = "Message here";

        // authen

        public const string ACCOUNT_NEED_CONFIRM_EMAIL = "AccountDoesNotVerifyEmail";
        public const string WRONG_PASSWORD = "PasswordIsIncorrect";
        public const string TOKEN_NOT_VALID = "TokenNotValid";
        public const string OTP_INVALID = "OtpInvalid";
        public const string OLD_PASSWORD_INVALID = "OldPasswordInvalid";
        public const string RESET_PASSWORD_FAILED = "CanNotResetPassword";
        public const string ACCOUNT_VERIFIED = "AccountVerified";

        public const string LOGIN_SUCCESS_MESSAGE = "Login successfully";
        public const string LOGIN_GOOGLE_SUCCESS_MESSAGE = "Login with google successfully";
        public const string TOKEN_REFRESH_SUCCESS_MESSAGE = "Token refresh successfully";
        public const string REGISTER_SUCCESS_MESSAGE = "The OTP has been sent to your email. Please verify it to log in.";
        public const string CHANGE_PASSWORD_SUCCESS = "Change password successfully";
        public const string REQUEST_RESET_PASSWORD_SUCCESS_MESSAGE = "The OTP reset password has been sent to your email";
        public const string REQUEST_RESET_PASSWORD_CONFIRM_SUCCESS_MESSAGE = "You can reset password now";

        // account
        public const string ACCOUNT_NOT_EXIST = "AccountNotExist";
        public const string ACCOUNT_EXISTED = "AccountAlreadyExisted";
        public const string ACCOUNT_BLOCKED = "AccountWasBlocked";
        public const string ACCOUNT_NOT_ENOUGH_AGE = "AccountMust16Age";
        public const string DUPLICATE_PHONE_NUMBER = "DuplicatePhoneNumber";
        public const string ACCOUNT_CURRENT_USER = "AccountIsCurrentUser";

        public const string ACCOUNT_CREATED_SUCCESS_MESSAGE = "Account created";
        public const string ACCOUNT_UPDATE_SUCCESS_MESSAGE = "Updated user successfully";
        public const string ACCOUNT_DELETE_SUCCESS_MESSAGE = "Deleted user successfully";
        public const string ACCOUNT_BAN_SUCCESS_MESSAGE = "Banned user successfully";

        // Spending model
        public const string SPENDING_MODEL_LIST_FETCHED_SUCCESS = "SpendingModelListFetched"; //Spending model list fetched successfully.
        public const string SPENDING_MODEL_FETCHED_SUCCESS = "SpendingModelFetched"; // Spending model details fetched successfully."
        public const string SPENDING_MODEL_CREATED_SUCCESS = "Spending model created successfully.";
        public const string SPENDING_MODEL_UPDATED_SUCCESS = "Spending model updated successfully.";
        public const string SPENDING_MODEL_DELETED_SUCCESS = "Spending model deleted successfully.";

        public const string SPENDING_MODEL_NOT_FOUND = "SpendingModelNotFound";
        public const string SPENDING_MODEL_ALREADY_EXISTS = "SpendingModelAlreadyExists";
        public const string SPENDING_MODEL_HAS_DEPENDENCIES = "SpendingModelHasDependencies";//The spending model has dependencies and cannot be deleted.
        public const string DUPLICATE_SPENDING_MODELS = "DuplicateSpendingModels";//Duplicate spending models found in the provided list.
        public const string EMPTY_SPENDING_MODEL_LIST = "EmptySpendingModelList";//The spending model list is empty.
        public const string DUPLICATE_CATEGORY_IDS_IN_LIST = "DuplicateCategoryIdsInList";//Duplicate category IDs found in the provided list.
        public const string CATEGORIES_ALREADY_ADDED = "CategoriesAlreadyAdded";//All categories in the list are already added to the spending model.
        public const string CATEGORY_NOT_FOUND_IN_SPENDING_MODEL = "CategoryNotFoundInSpendingModel";//The category was not found in the spending model.
        public const string CATEGORIES_NOT_FOUND_IN_SPENDING_MODEL = "CategoriesNotFoundInSpendingModel";//None of the categories were found in the spending model.
        public const string DUPLICATE_CATEGORY_IDS_IN_REQUEST = "DuplicateCategoryIdsInRequest";//Duplicate category IDs found in the request.
        public const string INVALID_CATEGORY_IDS = "InvalidCategoryIds";//Some category IDs do not exist in the database.
        public const string EMPTY_CATEGORY_LIST = "EmptyCategoryList";//The list of category IDs cannot be empty.
        public const string INVALID_TOTAL_PERCENTAGE = "InvalidTotalPercentage";//The total percentage amount of all categories must be greater than 0 and less than or equal to 100.
        public const string PERCENTAGE_REQUIRED = "PercentageRequired";//Percentage amounts must be provided for the categories.
        public const string PERCENTAGE_MISMATCH = "PercentageMismatch"; //The number of percentage amounts must match the number of category IDs.

        // category
        public const string CATEGORY_ALREADY_EXISTS = "CategoryAlreadyExists";
        public const string CATEGORY_CREATED_SUCCESS = "CategoryCreatedSuccessfully";
        public const string CATEGORY_NOT_FOUND = "CategoryNotFound";
        public const string CATEGORY_LIST_FETCHED_SUCCESS = "Category list fetched successfully.";
        public const string CATEGORY_FETCHED_SUCCESS = "Category details fetched successfully.";
        public const string CATEGORY_UPDATED_SUCCESS = "CategoryUpdatedSuccessfully";
        public const string CATEGORY_DELETED_SUCCESS = "CategoryDeletedSuccessfully";
        public const string CATEGORY_HAS_DEPENDENCIES = "CategoryHasDependencies";

    }
}
