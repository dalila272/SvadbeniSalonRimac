using Microsoft.AspNetCore.Authorization;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize]
public class RecenzijeController
    : BaseCRUDController<RecenzijaResponse, RecenzijaSearchObject, RecenzijaInsertRequest, RecenzijaUpdateRequest, IRecenzijaService>
{
    public RecenzijeController(IRecenzijaService service) : base(service)
    {
    }
}
