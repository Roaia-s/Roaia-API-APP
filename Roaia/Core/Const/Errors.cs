namespace Roaia.Core.Const;

public static class Errors
{
	public const string RequiredField = "Required field*";
	public const string NotAllowedExtensions = "Only (*.jpg *.jpeg *.png) files are allowed!";
	public const string MaxSize = "File cannot be more than 4 MB!";
	public const string MaxLength = "Max length cannot be more than {1} letters!";
	public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long!";
	public const string Duplicated = "Another record with the same {0} is already exists!";
	public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match!";
	public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long!";
	public const string InvalidUsername = "Username can only contain letters or digits!";
	public const string OnlyEnglishLetters = "Only English letters are allowed.";
	public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
	public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
	public const string DenySpecialCharacters = "Special characters are not allowed.";
	public const string InvalidMobileNumber = "Invalid mobile number.";
	public const string InvalidNationalId = "Invalid national ID.";
}
