﻿namespace Owin.Scim.Endpoints.Users
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Newtonsoft.Json.Serialization;

    using Configuration;
    using Extensions;
    using Model;
    using Model.Groups;
    using Patching;
    using Services;

    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupsController(
            ScimServerConfiguration scimServerConfiguration,
            IGroupService groupService) 
            : base(scimServerConfiguration)
        {
            _groupService = groupService;
        }

        [Route("groups", Name = "CreateGroup")]
        public async Task<HttpResponseMessage> Post(Group group)
        {
            return (await _groupService.CreateGroup(group))
                .ToHttpResponseMessage(Request, (groupDto, response) =>
                {
                    response.StatusCode = HttpStatusCode.Created;

                    SetLocationHeader(response, groupDto, "RetrieveGroup", new { groupId = groupDto.Id });
                    SetETagHeader(response, groupDto);
                });
        }

        [Route("groups/{groupId}", Name = "RetrieveGroup")]
        public async Task<HttpResponseMessage> Get(string groupId)
        {
            return (await _groupService.RetrieveGroup(groupId))
                .ToHttpResponseMessage(Request, (groupDto, response) =>
                {
                    SetLocationHeader(response, groupDto, "RetrieveGroup", new { groupId = groupDto.Id });
                    SetETagHeader(response, groupDto);
                });
        }

        [AcceptVerbs("PUT", "OPTIONS")]
        [Route("groups/{groupId}", Name = "ReplaceGroup")]
        public async Task<HttpResponseMessage> Put(string groupId, Group group)
        {
            if (String.IsNullOrWhiteSpace(groupId) ||
                group == null ||
                string.IsNullOrWhiteSpace(group.Id) ||
                !group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))
            {
                return new ScimErrorResponse<Group>(
                    new ScimError(
                        HttpStatusCode.BadRequest,
                        ScimErrorType.InvalidSyntax,
                        detail: "The request path 'groupId' MUST match the group.id in the request body."))
                    .ToHttpResponseMessage(Request);
            }

            return (await _groupService.UpdateGroup(group))
                .ToHttpResponseMessage(Request, (groupDto, response) =>
                {
                    SetLocationHeader(response, groupDto, "RetrieveGroup", new {groupId = groupDto.Id});
                    SetETagHeader(response, groupDto);
                });
        }

        [Route("groups/{groupId}", Name = "DeleteGroup")]
        public async Task<HttpResponseMessage> Delete(string groupId)
        {
            return (await _groupService.DeleteGroup(groupId)).ToHttpResponseMessage(Request, HttpStatusCode.NoContent);
        }

        [Route("groups/{groupId}", Name = "UpdateGroup")]
        public async Task<HttpResponseMessage> Patch(string groupId, PatchRequest<Group> patchRequest)
        {
            if (patchRequest?.Operations == null ||
                patchRequest.Operations.Operations.Any(a => a.OperationType == Patching.Operations.OperationType.Invalid))
            {
                return new ScimErrorResponse<Model.Users.User>(
                    new ScimError(
                        HttpStatusCode.BadRequest,
                        ScimErrorType.InvalidSyntax,
                        "The patch request body is unparsable, syntactically incorrect, or violates schema."))
                    .ToHttpResponseMessage(Request);
            }

            return (await (await _groupService.RetrieveGroup(groupId))
                .Bind<Group, Group>(group =>
                {
                    try
                    {
                        // TODO: (DG) Finish patch support
                        var result = patchRequest.Operations.ApplyTo(
                            group,
                            new ScimObjectAdapter<Group>(new CamelCasePropertyNamesContractResolver()));

                        return new ScimDataResponse<Group>(group);
                    }
                    catch (Patching.Exceptions.ScimPatchException ex)
                    {
                        return new ScimErrorResponse<Group>(ex.ToScimError());
                    }
                })
                .BindAsync(group => _groupService.UpdateGroup(group)))
                .ToHttpResponseMessage(Request, (groupDto, response) =>
                {
                    SetLocationHeader(response, groupDto, "RetrieveGroup", new { userId = groupDto.Id });
                    SetETagHeader(response, groupDto);
                });
        }

    }
}