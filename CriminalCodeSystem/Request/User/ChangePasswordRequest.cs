using System;

namespace CriminalCodeSystem.Request.User;

public class ChangePasswordRequest
{
    public string AccessToken { get; set; } = "";
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}