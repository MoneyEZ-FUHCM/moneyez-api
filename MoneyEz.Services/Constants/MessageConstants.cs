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
        public const string OTP_HAS_SENT = "OtpHasSent";
        public const string EMAIL_NOT_REQUEST_OTP = "EmailNotRequestOtpCode";

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
        public const string ACCOUNT_UPDATE_TOKEN_FAILED = "UpdateDeviceTokenFailed";

        public const string ACCOUNT_CREATED_SUCCESS_MESSAGE = "Account created";
        public const string ACCOUNT_UPDATE_SUCCESS_MESSAGE = "Updated user successfully";
        public const string ACCOUNT_DELETE_SUCCESS_MESSAGE = "Deleted user successfully";
        public const string ACCOUNT_BAN_SUCCESS_MESSAGE = "Banned user successfully";
        public const string ACCOUNT_UPDATE_TOKEN_SUCCESS_MESSAGE = "Update device token successfully";

        //recurring transaction
        public const string RECURRING_TRANSACTION_CREATED_SUCCESS = "RecurringTransactionCreatedSuccess";
        public const string RECURRING_TRANSACTION_UPDATED_SUCCESS = "RecurringTransactionUpdatedSuccess";
        public const string RECURRING_TRANSACTION_DELETED_SUCCESS = "RecurringTransactionDeletedSuccess";
        public const string RECURRING_TRANSACTION_FETCHED_SUCCESS = "RecurringTransactionFetchedSuccess";
        public const string RECURRING_TRANSACTION_LIST_FETCHED_SUCCESS = "RecurringTransactionListFetchedSuccess";
        public const string RECURRING_TRANSACTION_NOT_FOUND = "RecurringTransactionNotFound";
        public const string RECURRING_TRANSACTION_ACCESS_DENIED = "RecurringTransactionAccessDenied";
        public const string RECURRING_TRANSACTION_DELETE_DENIED = "RecurringTransactionDeleteDenied";


        // Spending model
        public const string SPENDING_MODEL_LIST_FETCHED_SUCCESS = "SpendingModelListFetched"; //Spending model list fetched successfully.
        public const string SPENDING_MODEL_FETCHED_SUCCESS = "SpendingModelFetched"; // Spending model details fetched successfully."
        public const string SPENDING_MODEL_UPDATED_SUCCESS = "Spending model updated successfully.";
        public const string SPENDING_MODEL_DELETED_SUCCESS = "Spending model deleted successfully.";

        public const string SPENDING_MODEL_NOT_FOUND = "SpendingModelNotFound";
        public const string SPENDING_MODEL_ALREADY_EXISTS = "SpendingModelAlreadyExists";
        public const string SPENDING_MODEL_HAS_DEPENDENCIES = "SpendingModelHasDependencies";//The spending model has dependencies and cannot be deleted.
        public const string DUPLICATE_SPENDING_MODELS = "DuplicateSpendingModels";//Duplicate spending models found in the provided list.
        public const string EMPTY_SPENDING_MODEL_LIST = "EmptySpendingModelList";//The spending model list is empty.
        public const string CATEGORIES_ALREADY_ADDED = "CategoriesAlreadyAdded";//All categories in the list are already added to the spending model.
        public const string CATEGORY_NOT_FOUND_IN_SPENDING_MODEL = "CategoryNotFoundInSpendingModel";//The category was not found in the spending model.
        public const string CATEGORIES_NOT_FOUND_IN_SPENDING_MODEL = "CategoriesNotFoundInSpendingModel";//None of the categories were found in the spending model.
        public const string DUPLICATE_CATEGORY_IDS_IN_REQUEST = "DuplicateCategoryIdsInRequest";//Duplicate category IDs found in the request.
        public const string INVALID_CATEGORY_IDS = "InvalidCategoryIds";//Some category IDs do not exist in the database.
        public const string EMPTY_CATEGORY_LIST = "EmptyCategoryList";//The list of category IDs cannot be empty.
        public const string INVALID_TOTAL_PERCENTAGE = "InvalidTotalPercentage";//The total percentage amount of all categories must be greater than 0 and less than or equal to 100.
        public const string PERCENTAGE_REQUIRED = "PercentageRequired";//Percentage amounts must be provided for the categories.
        public const string INVALID_PERCENTAGE_AMOUNT = "Percentage amounts cannot be negative.";
        public const string INVALID_PERIOD_UNIT = "InvalidPeriodUnit";
        public const string USER_ALREADY_HAS_ACTIVE_SPENDING_MODEL = "UserAlreadyHasActiveSpendingModel";
        public const string CURRENT_SPENDING_MODEL_NOT_FINISHED = "CurrentSpendingModelNotFinished";
        public const string CANNOT_CANCEL_SPENDING_MODEL_HAS_GOALS = "CannotCancelSpendingModelHasGoals";
        public const string START_DATE_CANNOT_BE_IN_PAST = "StartDateCannotBeInPast";
        public const string END_DATE_MUST_BE_AFTER_START_DATE = "EndDateMustBeAfterStartDate";
        public const string CANNOT_SELECT_FUTURE_MODEL_WHEN_ACTIVE = "CannotSelectFutureModelWhenActive";
        public const string USER_SPENDING_MODEL_ACCESS_DENY = "UserSpendingModelAccessDeny";

        //financial goal
        public const string USER_HAS_NO_ACTIVE_SPENDING_MODEL = "UserHasNoActiveSpendingModel";
        public const string SUBCATEGORY_NOT_IN_SPENDING_MODEL = "SubcategoryNotInSpendingModel";
        public const string SUBCATEGORY_ALREADY_HAS_GOAL = "SubcategoryAlreadyHasGoal";
        public const string FINANCIAL_GOAL_NOT_FOUND = "FinancialGoalNotFound";
        public const string FINANCIAL_GOAL_ACCESS_DENIED = "FinancialGoalAccessDenied";
        public const string INVALID_TARGET_AMOUNT = "InvalidTargetAmount";
        public const string INVALID_DEADLINE = "InvalidDeadline";
        public const string SPENDING_MODEL_DATA_MISSING = "SpendingModelDataMissing";

        public const string USER_NOT_IN_GROUP = "UserNotInGroup";
        public const string USER_NOT_AUTHORIZED = "UserNotAuthorized";
        public const string GROUP_ALREADY_HAS_GOAL = "GroupAlreadyHasGoal";
        public const string GROUP_NOT_FOUND = "GroupNotFound";
        public const string FINANCIAL_GOAL_NOT_IN_GROUP = "FinancialGoalNotInGroup";
        public const string INSUFFICIENT_GROUP_FUNDS = "InsufficientGroupFunds";
        public const string GOAL_NOT_COMPLETED = "GoalNotCompleted";
        public const string INVALID_SPENDING_MODEL = "InvalidSpendingModel";
        public const string SPENDING_MODEL_HAS_NO_CATEGORIES = "SpendingModelHasNoCategories";
        public const string SPENDING_MODEL_HAS_NO_SUBCATEGORIES = "SpendingModelHasNoSubcategories";
        public const string FINANCIAL_GOAL_CANNOT_BE_DELETED = "FinancialGoalCannotBeDeleted";
        public const string GOAL_ALREADY_COMPLETED = "GoalAlreadyCompleted";
        //financial report
        public const string REPORT_NOT_FOUND = "ReportNotFound";
        public const string REPORT_CREATE_FAILED = "ReportCreateFailed";
        public const string REPORT_UPDATE_FAILED = "ReportUpdateFailed";
        public const string REPORT_DELETE_FAILED = "ReportDeleteFailed";
        public const string REPORT_ACCESS_DENIED = "ReportAccessDenied";
        public const string INVALID_REPORT_DATE_RANGE = "InvalidReportDateRange";
        public const string REPORT_USER_NOT_FOUND = "ReportUserNotFound";
        public const string REPORT_GROUP_NOT_FOUND = "ReportGroupNotFound";
        public const string REPORT_TRANSACTION_ERROR = "ReportTransactionError";

        public const string REPORT_GENERATE_SUCCESS_MESSAGE = "Financial report generated successfully.";
        public const string REPORT_FETCHED_SUCCESS_MESSAGE = "Financial report retrieved successfully.";
        public const string REPORT_LIST_FETCHED_SUCCESS_MESSAGE = "Financial report list retrieved successfully.";
        public const string REPORT_UPDATED_SUCCESS_MESSAGE = "Financial report updated successfully.";
        public const string REPORT_DELETED_SUCCESS_MESSAGE = "Financial report deleted successfully.";

        public const string REPORT_NAME_REQUIRED = "ReportNameRequired";
        public const string REPORT_START_DATE_REQUIRED = "ReportStartDateRequired";
        public const string REPORT_END_DATE_REQUIRED = "ReportEndDateRequired";
        public const string REPORT_TYPE_REQUIRED = "ReportTypeRequired";
        public const string REPORT_GROUP_ID_REQUIRED = "ReportGroupIdRequired";

        public const string REPORT_NAME_REQUIRED_MESSAGE = "Report name is required.";
        public const string REPORT_START_DATE_REQUIRED_MESSAGE = "Start date is required.";
        public const string REPORT_END_DATE_REQUIRED_MESSAGE = "End date is required.";
        public const string REPORT_TYPE_REQUIRED_MESSAGE = "Report type is required.";
        public const string REPORT_GROUP_ID_REQUIRED_MESSAGE = "Group ID is required for group reports.";
        public const string REPORT_PERMISSION_DENIED = "ReportPermissionDenied";
        public const string REPORT_PERMISSION_DENIED_MESSAGE = "You do not have permission to access this report.";

        // category
        public const string CATEGORY_ALREADY_EXISTS = "CategoryAlreadyExists";
        public const string CATEGORY_CREATED_SUCCESS = "CategoryCreatedSuccessfully";
        public const string CATEGORY_NOT_FOUND = "CategoryNotFound";
        public const string CATEGORY_LIST_FETCHED_SUCCESS = "Category list fetched successfully.";
        public const string CATEGORY_FETCHED_SUCCESS = "Category details fetched successfully.";
        public const string CATEGORY_UPDATED_SUCCESS = "CategoryUpdatedSuccessfully";
        public const string CATEGORY_DELETED_SUCCESS = "CategoryDeletedSuccessfully";
        public const string CATEGORY_HAS_DEPENDENCIES = "CategoryHasDependencies";
        public const string CATEGORY_TYPE_INVALID = "CATEGORY_TYPE_INVALID";

        // subcategory
        public const string SUBCATEGORY_LIST_FETCHED_SUCCESS = "SubcategoryListFetched";
        public const string SUBCATEGORY_FETCHED_SUCCESS = "SubcategoryFetched";
        public const string SUBCATEGORY_CREATED_SUCCESS = "SubcategoryCreatedSuccessfully";
        public const string SUBCATEGORY_UPDATED_SUCCESS = "SubcategoryUpdatedSuccessfully";
        public const string SUBCATEGORY_DELETED_SUCCESS = "SubcategoryDeletedSuccessfully";
        public const string SUBCATEGORY_HAS_DEPENDENCIES = "SubcategoryHasDependencies";
        public const string INVALID_SUBCATEGORY_IDS = "InvalidSubcategoryIds";
        public const string SUBCATEGORY_NOT_FOUND = "SubcategoryNotFound";
        public const string SUBCATEGORY_ALREADY_EXISTS = "SubcategoryAlreadyExists";
        public const string DUPLICATE_SUBCATEGORY_NAMES = "DuplicateSubcategoryNames";
        public const string DUPLICATE_SUBCATEGORY_CODES = "DuplicateSubcategoryCodes";
        public const string EMPTY_SUBCATEGORY_LIST = "EmptySubcategoryList";
        public const string CATEGORY_ID_REQUIRED = "CategoryIdRequired";
        public const string SUBCATEGORY_NOT_FOUND_IN_CATEGORY = "SubcategoryNotFoundInCategory";
        public const string DUPLICATE_SUBCATEGORY_NAME_GLOBAL = "DuplicateSubcategoryNameGlobal";
        public const string DUPLICATE_SUBCATEGORY_NAME_IN_CATEGORY = "DuplicateSubcategoryNameInCategory";

        // transaction
        public const string TRANSACTION_CREATED_SUCCESS = "TransactionCreatedSuccessfully";
        public const string TRANSACTION_UPDATED_SUCCESS = "TransactionUpdatedSuccessfully";
        public const string TRANSACTION_DELETED_SUCCESS = "TransactionDeletedSuccessfully";
        public const string TRANSACTION_FETCHED_SUCCESS = "TransactionFetchedSuccessfully";

        public const string TRANSACTION_NOT_IN_GROUP = "TransactionDoesNotBelongToGroup";
        public const string TRANSACTION_RESPONSE_SUCCESS = "TransactionResponseSuccess";
        public const string TRANSACTION_MUST_BE_PENDING = "TransactionMustBePending";
        public const string TRANSACTION_APPROVE_DENIED = "TransactionApproveDenied";

        public const string TRANSACTION_ACCESS_DENIED = "TransactionAccessDenied";
        public const string TRANSACTION_CREATE_DENIED = "TransactionCreateDenied";
        public const string TRANSACTION_UPDATE_DENIED = "TransactionUpdateDenied";
        public const string TRANSACTION_DELETE_DENIED = "TransactionDeleteDenied";

        public const string TRANSACTION_LIST_FETCHED_SUCCESS = "TransactionListFetchedSuccessfully";
        public const string TRANSACTION_NOT_FOUND = "TransactionNotFound";
        public const string TRANSACTION_APPROVED_SUCCESS = "TransactionApprovedSuccessfully";
        public const string TRANSACTION_REJECTED_SUCCESS = "TransactionRejectedSuccessfully";
        public const string TRANSACTION_ALREADY_APPROVED = "TransactionAlreadyApproved";
        public const string TRANSACTION_ALREADY_REJECTED = "TransactionAlreadyRejected";
        public const string TRANSACTION_CANNOT_REJECT_SELF = "UserCannotRejectTheirOwnTransaction";
        public const string INVALID_TRANSACTION_REQUEST = "InvalidTransactionRequest";
        public const string TRANSACTION_ID_REQUIRED = "TransactionIdRequired";
        public const string TRANSACTION_AMOUNT_REQUIRED = "TransactionAmountRequired";
        public const string TRANSACTION_TYPE_INVALID = "TransactionTypeInvalid";
        public const string TRANSACTION_SUBCATEGORY_REQUIRED = "TransactionSubcategoryRequired";
        public const string TRANSACTION_DATE_REQUIRED = "TransactionDateRequired";
        public const string TRANSACTION_ADMIN_ACCESS_DENIED = "TransactionAdminAccessDenied";
        public const string SPENDING_MODEL_OVER_LIMIT = "SpendingModelOverLimit";
        public const string TRANSACTION_AMOUNT_INVALID = "TransactionAmountInvalid";
        public const string TRANSACTION_REJECTED_MISSING_REASON = "TransactionRejectMissingReason";

        //vote
        public const string VOTE_ALREADY_EXISTS = "VoteAlreadyExists";
        public const string VOTE_UPDATED = "VoteUpdated";
        public const string VOTE_SUCCESS = "VoteSuccess";
        public const string VOTE_NOT_FOUND = "VoteNotFound";
        public const string VOTE_DELETED = "VoteDeleted";
        public const string PERMISSION_DENIED = "PermissionDenied.";

        // group
        public const string GROUP_CREATE_SUCCESS_MESSAGE = "Group created successfully";
        public const string GROUP_GET_ALL_SUCCESS_MESSAGE = "Group get all successfully";
        public const string GROUP_CLOSE_FAIL = "GroupCloseFailed";
        public const string GROUP_CLOSE_SUCCESS_MESSAGE = "Group closed successfully";
        public const string GROUP_ACCESS_DENIED = "GroupAccessDenied";
        public const string GROUP_WITHDRAWAL_AMOUNT_INVALID = "GroupWithdrawalAmountInvalid";

        public const string GROUP_CLOSE_FORBIDDEN = "GroupCloseForbidden";
        public const string GROUP_REMOVE_MEMBER_FORBIDDEN = "GroupMemberNotFound";
        public const string GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE = "Group member removed successfully";
        public const string GROUP_SET_ROLE_FORBIDDEN = "GroupSetRoleForbidden";
        public const string GROUP_MEMBER_NOT_FOUND = "GroupMemberNotFound";
        public const string MEMBER_ROLE_UPDATE_SUCCESS_MESSAGE = "Group member role updated successfully";

        public const string GROUP_INVITE_SUCCESS_MESSAGE = "Invitation sent successfully.";
        public const string GROUP_INVITE_FORBIDDEN_MESSAGE = "Only the group leader can invite members.";
        public const string INVALID_INVITATION_TOKEN_MESSAGE = "Invalid invitation token.";
        public const string GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE = "Invitation accepted successfully.";
        public const string GROUP_NOT_EXIST = "GroupNotFound";
        public const string GROUP_MEMBER_EXIST = "GroupMemberAlreadyExist";
        public const string GROUP_CAN_NOT_REMOVE_LEADER = "YouAreTheLeader";
        public const string GROUP_LEAVE_SUCCESS_MESSAGE = "You have left the group successfully.";
        public const string GROUP_MEMBER_ALREADY_ROLE = "MemberAlreadyRole";
        public const string GROUP_LEADER_NOT_FOUND = "GroupLeaderNotFound";
        public const string GROUP_REMIND_FORBIDDEN = "GroupRemindForbidden";
        public const string GROUP_REMIND_SUCCESS_MESSAGE = "Group remind successfully";

        public const string GROUP_MEMBER_HAVE_TRANSACTION = "GroupMemberHaveTransaction";
        public const string GROUP_MEMBER_HAVE_TRANSACTION_MESSAGE = "Group member has transactions and cannot be removed.";

        // fundraising request
        public const string FUNDRAISING_REQUEST_NOT_FOUND = "FundraisingRequestNotFound";
        public const string FUNDRAISING_REQUEST_ACCESS_DENIED = "FundraisingRequestAccessDenied";
        public const string FUNDRAISING_REQUEST_LIST_GET_SUCCESS_MESSAGE = "Fundraising request list fetched successfully";
        public const string FUNDRAISING_REQUEST_GET_SUCCESS_MESSAGE = "Fundraising request details fetched successfully";


        // Group contribution
        public const string GROUP_SET_CONTRIBUTION_FORBIDDEN = "GroupSetContributionForbidden";
        public const string GROUP_INVALID_TOTAL_CONTRIBUTION = "GroupInvalidTotalContribution";
        public const string GROUP_MEMBER_CONTRIBUTION_NOT_FOUND = "GroupMemberContributionNotFound";
        public const string GROUP_SET_CONTRIBUTION_SUCCESS_MESSAGE = "Group contributions updated successfully";

        //subscription
        public const string SUBSCRIPTION_CREATE_SUCCESS_MESSAGE = "Subscription created successfully";

        // chat
        public const string CHAT_USER_NOT_EXIST = "UserNotExistChatConversation";

        // asset and liability
        public const string ASSET_LIST_GET_SUCCESS_MESSAGE = "Assets list get successfully";
        public const string ASSET_CREATED_SUCCESS = "AssetCreateSuccess";
        public const string ASSET_UPDATED_SUCCESS = "AssetUpdateSuccess";
        public const string ASSET_DELETED_SUCCESS = "AssetDeleteSuccess";
        public const string ASSET_NOT_FOUND = "AssetNotFound";

        public const string LIABILITY_LIST_GET_SUCCESS_MESSAGE = "Liabilities list get successfully";
        public const string LIABILITY_CREATED_SUCCESS = "LiabilityCreateSuccess";
        public const string LIABILITY_UPDATED_SUCCESS = "LiabilityUpdateSuccess";
        public const string LIABILITY_DELETED_SUCCESS = "LiabilityDeleteSuccess";
        public const string LIABILITY_NOT_FOUND = "LiabilityNotFound";

        // notification
        public const string NOTI_NOT_EXIST = "NotificationNotExist";
        public const string NOTI_PUSH_FAILED = "CannotPushNotification";
        public const string NOTI_UNREAD_EMPTY = "NotificationUnreadEmpty";
        public const string NOTI_CANNOT_MARK_READ = "CannotMarkReadNotification";
        public const string NOTI_USER_EMPTY = "NotificationUsersEmpty";

        // bank account
        public const string BANK_ACCOUNT_NOT_FOUND = "BankAccountNotFound";
        public const string BANK_ACCOUNT_ALREADY_EXISTS = "BankAccountAlreadyExists";
        public const string BANK_ACCOUNT_ACCESS_DENIED = "BankAccountAccessDenied";
        public const string BANK_ACCOUNT_NUMBER_DUPLICATE = "BankAccountNumberDuplicate";
        public const string BANK_ACCOUNT_REGISTERED_IN_GROUP = "BankAccountRegisteredInGroup";
        public const string BANK_ACCOUNT_VALIDATION_FAILED = "BankAccountValidationFailed";

        public const string BANK_ACCOUNT_LIST_GET_SUCCESS_MESSAGE = "Bank account list fetched successfully";
        public const string BANK_ACCOUNT_GET_SUCCESS_MESSAGE = "Bank account details fetched successfully";
        public const string BANK_ACCOUNT_CREATE_SUCCESS_MESSAGE = "Bank account created successfully";
        public const string BANK_ACCOUNT_UPDATE_SUCCESS_MESSAGE = "Bank account updated successfully";
        public const string BANK_ACCOUNT_DELETE_SUCCESS_MESSAGE = "Bank account deleted successfully";

        // webhook
        public const string WEBHOOK_REGISTRATION_FAILED = "WebhookRegistrationFailed";
        public const string WEBHOOK_URL_MISSING = "WebhookUrlMissing";
        public const string WEBHOOK_REGISTRATION_SUCCESS = "WebhookRegistrationSuccess";
        public const string WEBHOOK_REGISTRATION_SUCCESS_MESSAGE = "Webhook registered successfully";
        public const string WEBHOOK_SECRET_UPDATE_FAILED = "WebhookSecretUpdateFailed";
        public const string WEBHOOK_INVALID_RESPONSE = "WebhookInvalidResponse";
        public const string WEBHOOK_SERVER_ERROR = "WebhookServerError";
        public const string WEBHOOK_NOT_SUPPORTED = "WebhookNotSupported";
        public const string INVALID_WEBHOOK_SECRET = "InvalidWebhookSecret";
        public const string WEBHOOK_CANCELLATION_FAILED = "WebhookCancellationFailed";
        public const string WEBHOOK_NOT_REGISTERED = "WebhookNotRegistered";
        public const string WEBHOOK_SECRET_EXISTED = "WebhookSecretExisted";

        // external service
        public const string INVALID_EXTERNAL_SECRET = "InvalidExternalSecret";
        public const string INVALID_COMMAND = "InvalidCommand";
        public const string MISSING_PARAMETER = "MissingParameter";
        public const string INVALID_PARAMETER_FORMAT = "InvalidParameterFormat";

        // Post
        public const string POST_NOT_FOUND = "PostNotFound";
        public const string POST_ALREADY_EXISTS = "PostAlreadyExists";
        public const string POST_ACCESS_DENIED = "PostAccessDenied";
        public const string POST_CREATED_SUCCESS = "PostCreatedSuccessfully";
        public const string POST_UPDATED_SUCCESS = "PostUpdatedSuccessfully";
        public const string POST_DELETED_SUCCESS = "PostDeletedSuccessfully";
        public const string POST_LIST_FETCHED_SUCCESS = "Post list fetched successfully.";
        public const string POST_FETCHED_SUCCESS = "Post details fetched successfully.";
    }
}