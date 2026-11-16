using AstrologyMatches.Profiles.Application.Dtos;
using AstrologyMatches.Profiles.Application.Services;
using AstrologyMatches.Profiles.Domain;
using AstrologyMatches.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AstrologyMatches.Profiles.Infrastructure.Controllers;

/// <summary>
/// Фотографии профиля: загрузка, удаление, выбор главного и просмотр.
/// </summary>
[ApiController]
[Authorize]
[Route("api/profiles")]
public class ProfilePhotosController : ControllerBase
{
    // Запас сверх размера файла на служебные части multipart-запроса
    private const long MaxUploadRequestBytes = ProfilePhoto.MaxFileSizeBytes + 1024 * 1024;

    private readonly IProfilePhotoService _photoService;

    public ProfilePhotosController(IProfilePhotoService photoService)
    {
        _photoService = photoService;
    }

    /// <summary>
    /// Мои фото в порядке карусели, со ссылками на файлы.
    /// </summary>
    [HttpGet("me/photos")]
    [ProducesResponseType(typeof(IReadOnlyList<ProfilePhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProfilePhotoDto>>> GetMyPhotosAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProfilePhotoDto> photos = await _photoService.GetMyPhotosAsync(User.GetUserId(), cancellationToken);
        return Ok(photos);
    }

    /// <summary>
    /// Фото другого профиля (карточка кандидата, профиль собеседника).
    /// </summary>
    [HttpGet("{profileId:guid}/photos")]
    [ProducesResponseType(typeof(IReadOnlyList<ProfilePhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProfilePhotoDto>>> GetPhotosAsync(Guid profileId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProfilePhotoDto> photos = await _photoService.GetPhotosAsync(profileId, cancellationToken);
        return Ok(photos);
    }

    /// <summary>
    /// Загружает фото (JPEG, PNG или WEBP, до 5 МБ, не больше 6 фото на профиль).
    /// </summary>
    [HttpPost("me/photos")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadRequestBytes)]
    [ProducesResponseType(typeof(ProfilePhotoDto), StatusCodes.Status201Created)]
    // 400 и на слишком большой запрос: [ApiController] превращает ошибку чтения формы в ошибку валидации
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfilePhotoDto>> UploadPhotoAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using Stream content = file.OpenReadStream();
        ProfilePhotoDto photo = await _photoService.UploadAsync(User.GetUserId(), content, cancellationToken);
        return CreatedAtAction(nameof(GetMyPhotosAsync), photo);
    }

    /// <summary>
    /// Удаляет моё фото. Если оно было главным, главным становится следующее.
    /// </summary>
    [HttpDelete("me/photos/{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhotoAsync(Guid photoId, CancellationToken cancellationToken)
    {
        await _photoService.DeleteAsync(User.GetUserId(), photoId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Делает фото главным.
    /// </summary>
    [HttpPut("me/photos/{photoId:guid}/main")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMainPhotoAsync(Guid photoId, CancellationToken cancellationToken)
    {
        await _photoService.SetMainAsync(User.GetUserId(), photoId, cancellationToken);
        return NoContent();
    }
}
