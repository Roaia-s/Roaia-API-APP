namespace Roaia.Services;

public class AccountService(UserManager<ApplicationUser> userManager,
	ApplicationDbContext context,
	IImageService imageService) : IAccountService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly ApplicationDbContext _context = context;
	private readonly IImageService _imageService = imageService;

	public async Task<UserInfoDto> GetUserInformationAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
			return new UserInfoDto { Message = "This User Does Not  Exist" };

		UserInfoDto userInfo = new();

		userInfo.UserName = user.UserName;
		userInfo.Email = user.Email;
		userInfo.PhoneNumber = user.PhoneNumber;
		userInfo.FirstName = user.FirstName;
		userInfo.LastName = user.LastName;
		userInfo.ImageUrl = user.ImageUrl;
		userInfo.BlindId = user.GlassesId;

		return userInfo;
	}

	public async Task<BlindInfoDto> GetBlindInformationAsync(string blindId)
	{
		var blind = await _context.Glasses.FindAsync(blindId);
		if (blind is null)
			return new BlindInfoDto { Message = "This Id Does Not  Exist" };

		var diseases = await _context.Diseases.Where(d => d.GlassesId == blindId).OrderBy( d => d.Name).ToListAsync();

		BlindInfoDto blindInfo = new();
		blindInfo.Id = blindId;
		blindInfo.FullName = blind.FullName!;
		blindInfo.Age = blind.Age;
		blindInfo.Gender = blind.Gender!;
		blindInfo.ImageUrl = blind.ImageUrl!;
		blindInfo.Diseases = diseases.Select(d => d.Name).ToList();
		 
		return blindInfo;
	}

	public async Task<BlindInfoDto> ModifyBlindInfoAsync(BlindInfoDto dto)
	{
		var blind = await _context.Glasses.Include(d => d.Diseases).SingleOrDefaultAsync(g => g.Id == dto.Id);

		if (blind is null)
			return new BlindInfoDto { Message = "This Id Does Not  Exist" };

		if (dto.ImageUpload is not null)
		{
			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
			var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUpload,
				imageName,
				$"/images/blind", hasThumbnail: false);

			if (!isUploaded)
				return new BlindInfoDto { Message = errorMessage };

			//to do delete old image    Done ☻
			if (!string.IsNullOrEmpty(blind.ImageUrl))
				_imageService.Delete(blind.ImageUrl);

			blind.ImageUrl = $"/images/blind/{imageName}";
		}

		blind.FullName = dto.FullName;
		blind.Age = dto.Age;
		blind.Gender = dto.Gender;

		if (dto.Diseases is not null)
			blind.Diseases = dto.Diseases.Select(d => new Disease { Name = d, GlassesId = dto.Id }).ToList();

		await _context.SaveChangesAsync();

		return dto;
	}

	public async Task<ContactDto> ModifyContactInfoAsync(ContactDto dto)
	{
		var contact = await _context.Contacts.SingleOrDefaultAsync(c => c.FullName == dto.Name && c.GlassesId == dto.BlindId);
		if (contact is null)
			return new ContactDto { Message = "This Contact Does Not  Exist" };

		if (dto.ImageUpload is not null)
		{
			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
			var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUpload, imageName,
			$"/images/contact", hasThumbnail: false);

			if (!isUploaded)
				return new ContactDto { Message = errorMessage };

			//to do delete old image    Done ☻
			if (!string.IsNullOrEmpty(contact.ImageUrl))
				_imageService.Delete(contact.ImageUrl);

			contact.ImageUrl = $"/images/contact/{imageName}";
		}

		contact.FullName = dto.Name;
		contact.Age = dto.Age;
		contact.Relation = dto.Relation;

		await _context.SaveChangesAsync();

		return dto;
	}

	public async Task<ContactDto> AddContactAsync(ContactDto dto)
	{
		var blind = await _context.Glasses.FindAsync(dto.BlindId);

		if (blind is null)
			return new ContactDto { Message = "This Id Does Not  Exist" };

		

		if (dto.ImageUpload is not null)
		{
			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
			var (isUploaded, errorMessage) = await _imageService.UploadAsync(dto.ImageUpload,
								imageName,$"/images/contact", hasThumbnail: false);

			if (!isUploaded)
				return new ContactDto { Message = errorMessage };

			dto.ImageUrl = $"/images/contact/{imageName}";
		}

		var contact = new Contact
		{
			FullName = dto.Name,
			Age = dto.Age,
			Relation = dto.Relation,
			ImageUrl = !string.IsNullOrEmpty(dto.ImageUrl) ? dto.ImageUrl : $"/images/avatar.png",
			GlassesId = dto.BlindId
		};

		await _context.Contacts.AddAsync(contact);
		await _context.SaveChangesAsync();

		return dto;
	}

	public async Task<List<Contact>> GetAllContactsByIdAsync(string blindId)
	{
		var contacts = await _context.Contacts.Where(c => c.GlassesId == blindId).ToListAsync();
		return contacts;
	}
}
