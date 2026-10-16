using FluentResults;
using MeOne.Application.Communication.Suppliers.Dtos;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class AddSupplierDocumentUseCase(
    ISupplierRepository suppliers,
    ISupplierFileStorage storage,
    ICurrentActor currentActor) : IAddSupplierDocumentUseCase
{
    private static readonly string[] MissingItems =
    [
        SupplierErrors.ToolNotFound,
        SupplierErrors.MaterialNotFound,
        SupplierErrors.SelfAssessmentNotFound,
        SupplierErrors.AuditNotFound,
    ];

    public async Task<Result<SupplierDocumentResponse>> ExecuteAsync(
        ulong supplierId,
        AddSupplierDocumentRequest request,
        SupplierFileUpload? file,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult<SupplierDocumentResponse>();

        var (supplier, actorId) = loaded.Value;

        if (supplier.CheckDocumentTarget(request.Kind, request.OwnerId) is { } targetProblem)
        {
            return MissingItems.Contains(targetProblem)
                ? Result.Fail<SupplierDocumentResponse>(new NotFoundError(targetProblem))
                : Result.Fail<SupplierDocumentResponse>(targetProblem);
        }

        if (file is null)
            return Result.Fail<SupplierDocumentResponse>(SupplierErrors.FileRequired);

        if (file.Length <= 0)
            return Result.Fail<SupplierDocumentResponse>(SupplierErrors.FileEmpty);

        if (file.Length > SupplierErrors.MaxFileBytes)
            return Result.Fail<SupplierDocumentResponse>(SupplierErrors.FileTooLarge);

        if (file.FileName.Length is 0 or > SupplierErrors.MaxFileNameLength)
            return Result.Fail<SupplierDocumentResponse>(SupplierErrors.FieldTooLong("O nome do arquivo", SupplierErrors.MaxFileNameLength));

        var contentType = await ContentTypeOfAsync(file, cancellationToken);
        if (contentType is null)
            return Result.Fail<SupplierDocumentResponse>(SupplierErrors.FileInvalid);

        var storedName = await storage.SaveAsync(file.Content, SupplierDocumentRules.ExtensionFor(contentType), cancellationToken);

        try
        {
            var now = DateTime.UtcNow;

            var replaced = supplier.AttachDocument(
                request.Kind,
                request.OwnerId,
                file.FileName,
                storedName,
                contentType,
                file.Length,
                request.Version,
                request.ExpiresOn,
                actorId,
                now);

            supplier.MarkUpdated(actorId, now);
            suppliers.Update(supplier);

            if (await suppliers.SaveChangesAsync(cancellationToken) <= 0)
            {
                await storage.DeleteAsync(storedName, CancellationToken.None);
                return Result.Fail<SupplierDocumentResponse>(SupplierErrors.NothingPersisted);
            }

            foreach (var previous in replaced)
                await storage.DeleteAsync(previous.StoredName, CancellationToken.None);

            var created = supplier.Documents.Single(document => document.StoredName == storedName);

            return Result.Ok(SuppliersMapping.ToDocument(created, SuppliersMapping.Today()));
        }
        catch
        {
            await storage.DeleteAsync(storedName, CancellationToken.None);
            throw;
        }
    }

    private static async Task<string?> ContentTypeOfAsync(SupplierFileUpload file, CancellationToken cancellationToken)
    {
        if (!file.Content.CanSeek)
            return null;

        var buffer = new byte[FileSignatures.HeaderLength];
        file.Content.Position = 0;
        var read = await file.Content.ReadAsync(buffer, cancellationToken);
        file.Content.Position = 0;

        return SupplierDocumentRules.ImageOrPdfContentType(buffer.AsSpan(0, read));
    }
}
