namespace Assignmen_PRN232_1.Common
{
    public enum ApiStatusCode
    {
        Success = 200,
        Created = 201,

        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,

        InternalServerError = 500
    }
}
