﻿using Bit.Api.Auth.Validators;
using Bit.Api.Vault.Models.Request;
using Bit.Core.Entities;
using Bit.Core.Exceptions;
using Bit.Core.Vault.Entities;
using Bit.Core.Vault.Repositories;

namespace Bit.Api.Vault.Validators;

public class FolderRotationValidator : IRotationValidator<IEnumerable<FolderWithIdRequestModel>, IEnumerable<Folder>>
{
    private readonly IFolderRepository _folderRepository;

    public FolderRotationValidator(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<IEnumerable<Folder>> ValidateAsync(User user, IEnumerable<FolderWithIdRequestModel> folders)
    {
        var result = new List<Folder>();
        if (folders == null || !folders.Any())
        {
            return result;
        }

        var existingFolders = await _folderRepository.GetManyByUserIdAsync(user.Id);
        if (existingFolders == null || !existingFolders.Any())
        {
            return result;
        }

        foreach (var existing in existingFolders)
        {
            var folder = folders.FirstOrDefault(c => c.Id == existing.Id);
            if (folder == null)
            {
                throw new BadRequestException("All existing folders must be included in the rotation.");
            }
            result.Add(folder.ToFolder(existing));
        }
        return result;
    }
}
