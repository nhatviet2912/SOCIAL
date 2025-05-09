namespace Domain.Constants;

public static class ErrorMessageResponse
{
    public const string USER_NOT_FOUND = "User not found";
    public const string USER_ALREADY_EXIST = "User already exist";
    public const string USERNAME_ALREADY_EXIST = "UserName already exist";
    public const string USER_LOCKED = "User locked";
    public const string VALIDATION_ERROR = "Validation error";
    public const string AN_ERROR = "An error has occurred";
    public const string VALIDATION_FAILED = "Validation failed: {0}";
    public const string PASSWORD_INVALID = "Password invalid";
    public const string ROLE_NOT_FOUND = "Role not found";
    public const string ROLE_ALREADY_EXIST = "Role already exist";
    public const string IS_ROLE_EXIST = "Is role exist";
    public const string INVALID_ROLES = "Invalid roles: {0}";
    public const string ALL_ROLES_ALREADY_ASSIGNED = "All roles already assigned";
}