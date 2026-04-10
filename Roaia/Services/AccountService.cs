namespace Roaia.Services;

public class AccountService(UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    IConfiguration configuration,
    IImageService imageService,
    INotificationService notificationService,
    IOptions<NotificationSettings> notificationSettings) : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _context = context;

    private readonly IConfiguration _configuration = configuration;
    private readonly IImageService _imageService = imageService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly NotificationSettings _notificationSettings = notificationSettings.Value;

    public async Task<UserInfoDto> GetUserInformationAsync(string email)
    {
        var userName = email.ToUpper();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => (u.NormalizedUserName == userName || u.NormalizedEmail == userName || u.PhoneNumber == userName) && !u.IsDeleted);

        if (user is null)
            return new UserInfoDto { Message = "This User Does Not  Exist" };

        UserInfoDto userInfo = new()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{user.ImageUrl}",
            BlindId = user.GlassesId,
            Roles = (await _userManager.GetRolesAsync(user)).ToList()
        };

        return userInfo;
    }

    public async Task<BlindInfoDto> GetBlindInformationAsync(string blindId)
    {
        var blind = await _context.Glasses.Include(d => d.Diseases).SingleOrDefaultAsync(g => g.Id == blindId);
        if (blind is null)
            return new BlindInfoDto { Message = "This Id Does Not  Exist" };

        BlindInfoDto blindInfo = new()
        {
            Id = blindId,
            FullName = blind.FullName!,
            Age = blind.Age,
            Gender = blind.Gender!,
            ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{blind.ImageUrl}",
            Diseases = blind.Diseases.Select(d => d.Name).ToList()
        };

        return blindInfo;
    }

    public async Task<BlindInfoDto> ModifyBlindInfoAsync(BlindInfoDto dto)
    {
        var blind = await _context.Glasses.SingleOrDefaultAsync(g => g.Id == dto.Id);

        if (blind is null)
            return new BlindInfoDto { Message = "This Id Does Not Exist" };

        if (dto.ImageUpload is not null)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUpload,
                imageName, "/images/blind", hasThumbnail: false);

            if (!isUploaded)
                return new BlindInfoDto { Message = errorMessage };

            if (!string.IsNullOrEmpty(blind.ImageUrl))
                _imageService.Delete(blind.ImageUrl);

            blind.ImageUrl = $"{FileSettings.blindImagesPath}/{imageName}";
        }

        blind.FullName = dto.FullName;
        blind.Age = dto.Age;
        blind.Gender = dto.Gender;

        if (dto.Diseases is not null)
        {
            var existingDiseases = await _context.Diseases
                                                 .Where(d => d.GlassesId == blind.Id)
                                                 .ToListAsync();

            var existingDiseaseNames = existingDiseases.Select(d => d.Name).ToList();
            var newDiseases = dto.Diseases.Except(existingDiseaseNames).ToList();
            var removedDiseases = existingDiseases.Where(d => !dto.Diseases.Contains(d.Name)).ToList();

            if (newDiseases.Any())
            {
                foreach (var diseaseName in newDiseases)
                    _context.Add(new Disease { Name = diseaseName, GlassesId = blind.Id });
            }

            if (removedDiseases.Any())
                _context.Diseases.RemoveRange(removedDiseases);
        }

        await _context.SaveChangesAsync();

        var orderedDiseases = await _context.Diseases
                                            .Where(d => d.GlassesId == blind.Id)
                                            .OrderBy(d => d.Name)
                                            .Select(d => d.Name)
                                            .ToListAsync();

        return new BlindInfoDto
        {
            Id = blind.Id,
            FullName = blind.FullName!,
            Age = blind.Age,
            Gender = blind.Gender!,
            ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{blind.ImageUrl}",
            Diseases = orderedDiseases,
        };
    }

    public async Task<string> GenerateGlassesIdAsync()
    {
        var glassesId = Guid.NewGuid().ToString().Substring(0, 30);
        var isExist = await _context.Glasses.AnyAsync(g => g.Id == glassesId);
        if (isExist)
            return await GenerateGlassesIdAsync();

        Glasses glasses = new()
        {
            Id = glassesId,
            ImageUrl = FileSettings.defaultImagesPath
        };

        await _context.AddAsync(glasses);
        await _context.SaveChangesAsync();

        return glassesId;
    }

    public async Task<ContactDto> ModifyContactInfoAsync(ContactDto dto)
    {
        var contact = await _context.Contacts.SingleOrDefaultAsync(c => ((c.Id == dto.Id || c.FullName == dto.Name) && !c.IsDeleted) && c.GlassesId == dto.BlindId);
        if (contact is null)
            return new ContactDto { Message = "This Contact Does Not  Exist" };

        if (dto.ImageUpload is not null)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUpload, imageName,
            FileSettings.contactImagesPath, hasThumbnail: false);

            if (!isUploaded)
                return new ContactDto { Message = errorMessage };

            //to do delete old image    Done ☻
            if (!string.IsNullOrEmpty(contact.ImageUrl))
                _imageService.Delete(contact.ImageUrl);

            contact.ImageUrl = $"{FileSettings.contactImagesPath}/{imageName}";
        }

        contact.FullName = dto.Name ?? contact.FullName;
        contact.Age = dto.Age ?? contact.Age;
        contact.Relation = dto.Relation ?? contact.Relation;
        contact.PhoneNumber = dto.PhoneNumber ?? contact.PhoneNumber;

        await _context.SaveChangesAsync();

        ContactDto contactDto = new()
        {
            Id = contact.Id,
            Name = contact.FullName!,
            Age = contact.Age,
            Relation = contact.Relation!,
            PhoneNumber = contact.PhoneNumber!,
            BlindId = contact.GlassesId,
            ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{contact.ImageUrl}"
        };

        return contactDto;
    }

    public async Task<ContactDto> AddContactAsync(ContactDto dto)
    {
        var blind = await _context.Glasses.Include(g => g.Contacts).FirstOrDefaultAsync(g => g.Id == dto.BlindId);

        if (blind is null)
            return new ContactDto { Message = "This Id Does Not  Exist" };

        // Check subscription expiry
        if (blind.SubscriptionExpiryDate.HasValue && blind.SubscriptionExpiryDate.Value < DateTime.UtcNow)
        {
            // Reset to free plan if subscription expired
            blind.MaxContacts = 7;
            blind.SubscriptionExpiryDate = null;
            var contacts = blind.Contacts.Skip(7).ToList();
            contacts.ForEach(c => c.IsDeleted = true);
            await _context.SaveChangesAsync();
        }

        // Check the current number of contacts
        var contactsCount = blind.Contacts.Count;

        if (contactsCount >= blind.MaxContacts)
            return new ContactDto { Message = "The number of contacts exceeds the limit of your current plan!" };

        if (dto.ImageUpload is not null)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
            var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUpload,
                                imageName, FileSettings.contactImagesPath, hasThumbnail: false);

            if (!isUploaded)
                return new ContactDto { Message = errorMessage };

            dto.ImageUrl = $"{FileSettings.contactImagesPath}/{imageName}";
        }

        var contact = new Contact
        {
            FullName = dto.Name,
            Age = dto.Age,
            Relation = dto.Relation,
            PhoneNumber = dto.PhoneNumber,
            ImageUrl = !string.IsNullOrEmpty(dto.ImageUrl) ? dto.ImageUrl : FileSettings.defaultImagesPath,
            GlassesId = dto.BlindId
        };

        await _context.AddAsync(contact);
        await _context.SaveChangesAsync();

        // send notification to the blind
        await _notificationService.SendNotificationAsync(new NotificationDto
        {
            Title = _notificationSettings.Title,
            Body = $"{_notificationSettings.Body} {contact.FullName}. Make a positive impact on their journey!",
            ImageUrl = _notificationSettings.ImageUrl,
            AudioUrl = _notificationSettings.AudioUrl,
            GlassesId = contact.GlassesId,
            Type = _notificationSettings.Type
        });


        ContactDto contactDto = new()
        {
            Id = contact.Id,
            Name = contact.FullName!,
            Age = contact.Age,
            Relation = contact.Relation!,
            PhoneNumber = contact.PhoneNumber!,
            BlindId = contact.GlassesId,
            ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{contact.ImageUrl}"
        };

        return contactDto;
    }
    //
    public async Task<List<Contact>?> GetAllContactsByIdAsync(string blindId)
    {
        var contacts = await _context.Contacts.Where(c => c.GlassesId == blindId && !c.IsDeleted).AsNoTracking().ToListAsync();
        if (contacts.Count is 0)
            return null;

        // add domain to image for contacts
        contacts.ForEach(c => c.ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{c.ImageUrl}");

        return contacts;
    }

    public async Task<List<ContactImagesDto>?> GetAllImagesByGlassesIdAsync(string blindId)
    {
        var contacts = await _context.Contacts.Where(c => c.GlassesId == blindId).AsNoTracking().ToListAsync();
        if (contacts.Count is 0)
            return null;

        var contactImages = contacts.Select(c => new ContactImagesDto
        {
            Name = c.FullName,
            ImageUrl = $"{_configuration.GetSection("Application:AppDomain").Value}{c.ImageUrl}"
        }).ToList();

        return contactImages;
    }

    public async Task<string> DeleteAccountAsync(string userId)
    {
        var message = string.Empty;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            return message = "This User Does Not  Exist";

        var tokens = await _context.DeviceTokens
            .Where(t => t.UserId == userId)
            .ExecuteDeleteAsync();

        //delete user
        await _userManager.DeleteAsync(user);
        //user.IsDeleted = true;
        //user.LastUpdatedOn = DateTime.Now;

        return message;
    }

    public async Task<NotificationDto> ManualNotificationAsync(NotificationDto request)
    {
        if (await _context.Glasses.SingleOrDefaultAsync(g => g.Id == request.GlassesId) is null)
            return new NotificationDto { Message = "This Id Does Not  Exist" };

        var notification = new AppNotification
        {
            Title = request.Title,
            Body = request.Body,
            ImageUrl = request.ImageUrl,
            AudioUrl = request.AudioUrl,
            GlassesId = request.GlassesId,
            Type = request.Type,
            IsRead = false
        };

        await _context.AddAsync(notification);
        await _context.SaveChangesAsync();

        return request;
    }

    public async Task<string> DeleteContactAsync(int contactId)
    {
        var message = string.Empty;
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact is null)
            return message = "This Contact Does Not  Exist";

        //delete Contact image from server
        if (!string.IsNullOrEmpty(contact.ImageUrl))
            _imageService.Delete(contact.ImageUrl);

        _context.Remove(contact);
        await _context.SaveChangesAsync();

        return message;
    }
}