namespace Owin.Scim.Tests.Integration.Groups
{
    using System.Net;

    using Model.Users;
    using Model.Groups;

    public class using_existing_user_and_group : using_a_scim_server
    {
        protected static ScimUser CreateUser(ScimUser user)
        {
            var response = Server
                .HttpClient
                .PostAsync("users", new ScimObjectContent<ScimUser>(user)).Result;

            return (response.StatusCode == HttpStatusCode.Created)
                ? response.Content.ScimReadAsAsync<ScimUser>().Result
                : null;
        }

        protected static ScimGroup CreateGroup(ScimGroup group)
        {
            var response = Server
                .HttpClient
                .PostAsync("groups", new ScimObjectContent<ScimGroup>(group)).Result;

            return response.StatusCode == HttpStatusCode.Created
                ? response.Content.ScimReadAsAsync<ScimGroup>().Result
                : null;
        }
    }
}