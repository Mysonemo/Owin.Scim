namespace Owin.Scim.Tests.Querying.Filtering
{
    using System.Collections.Generic;
    using System.Linq;

    using Machine.Specifications;

    using Model.Users;

    public class and_present_equals : when_parsing_a_filter_expression<User>
    {
        Establish context = () =>
        {
            Users = new List<User>
            {
                new User { UserName = "BJensen"},
                new User { UserName = "LSmith", Title = "Engineer", UserType = "Manager" },
                new User { UserName = "DGioulakis", Title = "Engineer", UserType = "Employee" }
            };

            FilterExpression = "title pr and userType eq \"employee\"";
        };

        It should_filter = () => Users.Single(Predicate).UserName.ShouldEqual("DGioulakis");
    }
}