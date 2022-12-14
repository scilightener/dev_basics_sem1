using EconomyBlog.Services;

namespace EconomyBlog.Controllers;

[HttpController("register")]
public class RegisterController : Controller
{
    [HttpPOST]
    public static ActionResult RegisterUser(Guid sessionId, string login, string password, string rememberMe = "off")
    {
        int id;
        try
        {
            id = new UserDao().Insert(login, Hashing.Hash(password));
            new UsersFavoriteTopicsDao().Insert(new UsersFavoriteTopics(id, 0, 0, 0, 0, 0));
        }
        catch (SqlException ex)
        {
            return ex.Class switch
            {
                14 => new ErrorResult($"Login '{login}' is already taken. Consider another variant and try again."),
                16 => new ErrorResult($"Too long login. Only logins with maximum length 20 characters allowed."),
                _ => new ErrorResult(DbError)
            };
        }

        return new ActionResult
        {
            StatusCode = HttpStatusCode.Redirect,
            RedirectUrl = @"/home/edit/",
            Cookies = new CookieCollection
            {
                new Cookie("SessionId",
                        SessionManager.CreateSession(id, login, DateTime.Now, rememberMe == "on").ToString(), "/")
                    { Expires = rememberMe == "on" ? DateTime.Now.AddDays(150) : DateTime.Now.AddHours(1) }
            }
        };
    }

    [HttpGET]
    public static ActionResult GetRegisterPage(Guid sessionId, string path) => ProcessStatic("register", path);
}