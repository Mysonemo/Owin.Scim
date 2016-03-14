﻿namespace Owin.Scim.Tests.Validation.Groups
{
    using FakeItEasy;
    using FluentValidation;

    using Machine.Specifications;

    using Configuration;
    using Model.Groups;
    using Model.Users;
    using Repository;
    using Scim.Extensions;
    using Scim.Validation;
    using Scim.Validation.Groups;

    public class when_creating_a_group
    {
        Establish context = () =>
        {
            var userRepository = A.Fake<IUserRepository>();
            var groupRepository = A.Fake<IGroupRepository>();
            var scimServerConfiguration = A.Fake<ScimServerConfiguration>();

            scimServerConfiguration.PublicOrigin = "http://local/Scim/V2/";
            A.CallTo(() => userRepository.GetUser(A<string>.Ignored)).ReturnsLazily((string id) => id == ValidUserId ? new User() : null);
            A.CallTo(() => groupRepository.GetGroup(A<string>.Ignored)).ReturnsLazily((string id) => id == ValidGroupId ? new Group() : null);

            Validator = new GroupValidator(new ResourceExtensionValidators(null), scimServerConfiguration, userRepository, groupRepository);
        };

        Because of = () =>
        {
            Result = Validator.ValidateCreateAsync(Group).Result.ToScimValidationResult();
        };

        private static IValidator Validator;

        protected static string ValidUserId = "validUser";
        protected static string ValidGroupId = "validGroup";
        protected static ValidationResult Result;
        protected static Group Group;
    }
}