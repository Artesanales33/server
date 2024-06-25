﻿#nullable enable
using System.Security.Claims;
using Bit.Api.SecretsManager.Controllers;
using Bit.Api.SecretsManager.Models.Request;
using Bit.Core.Entities;
using Bit.Core.Exceptions;
using Bit.Core.Models.Data.Organizations.OrganizationUsers;
using Bit.Core.Repositories;
using Bit.Core.SecretsManager.Commands.Requests.Interfaces;
using Bit.Core.Services;
using Bit.Test.Common.AutoFixture;
using Bit.Test.Common.AutoFixture.Attributes;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Bit.Api.Test.SecretsManager.Controllers;

[ControllerCustomize(typeof(RequestSMAccessController))]
[SutProviderCustomize]
public class RequestSMAccessControllerTests
{
    [Theory]
    [BitAutoData]
    public async Task RequestSMAccessFromAdminst_WhenSendingNoModel_ShouldThrowNotFoundException(
    User user, SutProvider<RequestSMAccessController> sutProvider)
    {
        // Arrange
        sutProvider.GetDependency<IUserService>().GetUserByPrincipalAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        sutProvider.GetDependency<IOrganizationRepository>().GetByIdentifierAsync(Arg.Any<string>()).ReturnsNullForAnyArgs();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sutProvider.Sut.RequestSMAccessFromAdmins(new RequestSMAccessRequestModel()));
    }

    [Theory]
    [BitAutoData]
    public async Task RequestSMAccessFromAdmins_WhenSendingValidData_ShouldSucceed(
    User user,
    RequestSMAccessRequestModel model,
    Core.AdminConsole.Entities.Organization org,
    ICollection<OrganizationUserUserDetails> orgUsers,
    SutProvider<RequestSMAccessController> sutProvider)
    {
        // Arrange
        sutProvider.GetDependency<IOrganizationRepository>().GetByIdAsync(model.OrganizationId).Returns(org);
        sutProvider.GetDependency<IUserService>().GetUserByPrincipalAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        sutProvider.GetDependency<IOrganizationUserRepository>().GetManyDetailsByOrganizationAsync(org.Id).Returns(orgUsers);

        // Act & Assert
        await sutProvider.Sut.RequestSMAccessFromAdmins(model);

        //Also check that the command was called
        await sutProvider.GetDependency<IRequestSMAccessCommand>()
            .Received(1)
            .SendRequestAccessToSM(org, orgUsers, user, model.EmailContent);
    }

    [Theory]
    [BitAutoData]
    public async Task RequestSMAccessFromAdmins_WhenUserInvalid_ShouldThrowBadRequestException(RequestSMAccessRequestModel model, SutProvider<RequestSMAccessController> sutProvider)
    {
        // Arrange
        sutProvider.GetDependency<IUserService>().GetUserByPrincipalAsync(Arg.Any<ClaimsPrincipal>()).ReturnsNullForAnyArgs();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sutProvider.Sut.RequestSMAccessFromAdmins(model));
    }

    [Theory]
    [BitAutoData]
    public async Task RequestSMAccessFromAdmins_WhenOrgInvalid_ShouldThrowNotFoundException(RequestSMAccessRequestModel model, User user, SutProvider<RequestSMAccessController> sutProvider)
    {
        // Arrange
        sutProvider.GetDependency<IOrganizationRepository>().GetByIdentifierAsync(Arg.Any<string>()).ReturnsNullForAnyArgs();
        sutProvider.GetDependency<IUserService>().GetUserByPrincipalAsync(Arg.Any<ClaimsPrincipal>()).ReturnsForAnyArgs(user);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sutProvider.Sut.RequestSMAccessFromAdmins(model));
    }
}
