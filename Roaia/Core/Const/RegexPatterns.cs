namespace Roaia.Core.Const;

public static class RegexPatterns
{
    public const string Password = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
    public const string Email = @"[A-Za-z0-9._-]+@[A-Za-z0-9]+.[A-Za-z]{2,4}";
    public const string UserName = "^[A-Za-z0-9-._@]*$";
    public const string CharactersOnly_Eng = "^[a-zA-Z-_ ]*$";
    public const string CharactersOnly_Ar = "^[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF ]*$";
    public const string NumbersAndChrOnly_ArEng = "^(?=.*[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z])[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z0-9 _-]+$";
    public const string DenySpecialCharacters = "^[^<>!#%$]*$";
    public const string MobileNumber = "^01[0,1,2,5]{1}[0-9]{8}$";
    public const string NationalId = "^[2,3]{1}[0-9]{13}$";
    public const string GUID = "^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{6}";
    public const string ImageUrl = @"(http(s?):)([/|.|w|s|-|:|?|=|%|&|(|)|+|,|A-Z|a-z|0-9|/|_|-])+\.(?:jpg|jpeg|png)";
    public const string AudioUrl = @"(http(s?):)([/|.|w|s|-|:|?|=|%|&|(|)|+|,|A-Z|a-z|0-9|/|_|-])+\.(?:mp3|wav|m4a)";
    public const string VideoUrl = @"(http(s?):)([/|.|w|s|-|:|?|=|%|&|(|)|+|,|A-Z|a-z|0-9|/|_|-])+\.(?:mp4)";
    public const string NotificationType = "Normal|Warning|Critical";

}
