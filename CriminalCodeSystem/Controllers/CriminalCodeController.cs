using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CriminalCodeSystem.Models;
using CriminalCodeSystem.Request.CriminalCode;
using CriminalCodeSystem.Response.CriminalCode;
using CriminalCodeSystem.Helpers;
using CriminalCodeSystem.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CriminalCodeSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class CriminalCodeController : ControllerBase
{
    private readonly ILogger<CriminalCodeController> _logger;

    public CriminalCodeController(ILogger<CriminalCodeController> logger)
    {
        _logger = logger;
    }

    [NonAction]
    private async Task<CriminalCode?> RetrieveCriminalCode(CriminalCodeContext context, int criminalCodeId, string criminalCodeName)
    {
        if (criminalCodeId > 0) {
            return await context.CriminalCodes.Where(row => row.Id == criminalCodeId).SingleOrDefaultAsync();
        }
        else if (!string.IsNullOrEmpty(criminalCodeName))
        {
            return await context.CriminalCodes.Where(row => row.Name == criminalCodeName).SingleOrDefaultAsync();
        }

        return null;
    }

    [Route("List")]
    [HttpGet]
    public async Task<ListResponse> ListAsync([FromQuery] ListRequest request)
    {
        ListResponse response = new ListResponse();
        CriminalCodeContext criminalCodeContext = new CriminalCodeContext();
        UserContext userContext = new UserContext();
        StatusContext statusContext = new StatusContext();

        #region Normalization of input data
        request.OrderName = request.OrderName.ToUpper();
        request.OrderDescription = request.OrderDescription.ToUpper();
        request.OrderPenalty = request.OrderPenalty.ToUpper();
        request.OrderPrisonTime = request.OrderPrisonTime.ToUpper();
        request.OrderCreateDate = request.OrderCreateDate.ToUpper();
        request.OrderUpdateDate = request.OrderUpdateDate.ToUpper();
        #endregion

        IQueryable<CriminalCode> partialQuery = criminalCodeContext.CriminalCodes
            .Where(row => true);

        #region Applying filters on query result
        if (!string.IsNullOrEmpty(request.FilterName))
        {
            partialQuery = partialQuery.Where(row => row.Name == request.FilterName);
        }
        if (!string.IsNullOrEmpty(request.FilterDescription))
        {
            partialQuery = partialQuery.Where(row => row.Description == request.FilterDescription);
        }
        if (request.FilterPenalty != default(Decimal))
        {
            partialQuery = partialQuery.Where(row => row.Penalty == request.FilterPenalty);
        }
        if (request.FilterPrisonTime != default(int))
        {
            partialQuery = partialQuery.Where(row => row.PrisonTime == request.FilterPrisonTime);
        }
        if (request.FilterCreateDate != DateTime.MinValue && request.FilterCreateDate != default(DateTime))
        {
            partialQuery = partialQuery.Where(row => row.CreateDate.Date == request.FilterCreateDate.Date);
        }
        if (request.FilterUpdateDate != DateTime.MinValue && request.FilterUpdateDate != default(DateTime))
        {
            partialQuery = partialQuery.Where(row => row.UpdateDate.Date == request.FilterUpdateDate.Date);
        }
        if (!string.IsNullOrEmpty(request.FilterStatus))
        {
            Status? found = await statusContext.Statuses.Where(row => row.Name == request.FilterStatus).FirstOrDefaultAsync();
            if (found != null)
            {
                partialQuery = partialQuery.Where(row => row.StatusId == found.Id);
            }
        }
        if (!string.IsNullOrEmpty(request.FilterCreateUserName))
        {
            User? found = await userContext.Users.Where(row => row.UserName == request.FilterCreateUserName).FirstOrDefaultAsync();
            if (found != null)
            {
                partialQuery = partialQuery.Where(row => row.CreateUserId == found.Id);
            }
        }
        if (!string.IsNullOrEmpty(request.FilterUpdateUserName))
        {
            User? found = await userContext.Users.Where(row => row.UserName == request.FilterUpdateUserName).FirstOrDefaultAsync();
            if (found != null)
            {
                partialQuery = partialQuery.Where(row => row.UpdateUserId == found.Id);
            }
        }
        #endregion

        #region Ordering query result by specific fields
        if (request.OrderName == "ASC" || request.OrderName == "ASCENDING") {
            partialQuery = partialQuery.OrderBy(row => row.Name);
        }
        else if (request.OrderName == "DESC" || request.OrderName == "DESCENDING") {
            partialQuery = partialQuery.OrderByDescending(row => row.Name);
        }

        if (request.OrderDescription == "ASC" || request.OrderDescription == "ASCENDING") {
            partialQuery = partialQuery.OrderBy(row => row.Description);
        }
        else if (request.OrderDescription == "DESC" || request.OrderDescription == "DESCENDING") {
            partialQuery = partialQuery.OrderByDescending(row => row.Description);
        }

        if (request.OrderPenalty == "ASC" || request.OrderPenalty == "ASCENDING") {
            partialQuery = partialQuery.OrderBy(row => row.Penalty);
        }
        else if (request.OrderPenalty == "DESC" || request.OrderPenalty == "DESCENDING") {
            partialQuery = partialQuery.OrderByDescending(row => row.Penalty);
        }

        if (request.OrderPrisonTime == "ASC" || request.OrderPrisonTime == "ASCENDING") {
            partialQuery = partialQuery.OrderBy(row => row.PrisonTime);
        }
        else if (request.OrderPrisonTime == "DESC" || request.OrderPrisonTime == "DESCENDING") {
            partialQuery = partialQuery.OrderByDescending(row => row.PrisonTime);
        }

        if (request.OrderCreateDate == "ASC" || request.OrderCreateDate == "ASCENDING") {
            partialQuery = partialQuery.OrderBy(row => row.CreateDate);
        }
        else if (request.OrderCreateDate == "DESC" || request.OrderCreateDate == "DESCENDING") {
            partialQuery = partialQuery.OrderByDescending(row => row.CreateDate);
        }

        if (request.OrderUpdateDate == "ASC" || request.OrderUpdateDate == "ASCENDING") {
            partialQuery = partialQuery.OrderBy(row => row.UpdateDate);
        }
        else if (request.OrderUpdateDate == "DESC" || request.OrderUpdateDate == "DESCENDING") {
            partialQuery = partialQuery.OrderByDescending(row => row.UpdateDate);
        }
        #endregion

        #region Finally, we check pagination on query result
        if (request.NextId > 0)
        {
            partialQuery = partialQuery.Where(row => row.Id > request.NextId);
        }
        if (request.Limit > 0)
        {
            request.Limit = Math.Min(request.Limit, 100);
            partialQuery = partialQuery.Take(request.Limit);
        }
        if (request.Offset > 0)
        {
            request.Offset = Math.Min(request.Offset, 100);
            partialQuery = partialQuery.Skip(request.Offset);
        }
        #endregion

        List<CriminalCode> filtered = await partialQuery.ToListAsync();

        response.CriminalCodes = filtered;
        response.NextId = filtered.LastOrDefault()?.Id ?? -1;
        response.PageLength = filtered.Count();

        return response;
    }

    [Route("Add")]
    [HttpPost]
    public async Task<AddResponse> AddAsync(AddRequest request)
    {
        AddResponse response = new AddResponse();

        CriminalCodeContext criminalCodeContext = new CriminalCodeContext();
        UserContext userContext = new UserContext();
        StatusContext statusContext = new StatusContext();

        string accessToken = "";

        if (!string.IsNullOrEmpty(request.AccessToken))
        {
            accessToken = request.AccessToken;
        }
        else if (!string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]))
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            accessToken = authorization.Replace("Bearer ", "");
        }
        else
        {
            // this case would not be found due middleware behavior, but we ensure nevertheless the handling
            response.Status = -1;
            response.Message = "UNEXPECTED BEHAVIOR";
            return response;
        }

        int userId = 0;
        int statusId = 0;
        string tokenUserName = JwtUtils.GetUserNameFromJwt(accessToken);

        #region Loading associated data (both user and status)
        try {
            User? foundUser = await userContext.Users.SingleOrDefaultAsync(row => row.UserName == tokenUserName);
            userId = foundUser?.Id ?? 0;

            if (userId <= 0)
            {
                response.Status = -1;
                response.Message = "INCONSISTENT BASE - USER WAS REMOVED FROM STORE WITH ACTIVE TOKEN";
                return response;
            }
        }
        catch
        {
            response.Status = -1;
            response.Message = "INCONSISTENT BASE - TWO USERS OR MORE WITH THE SAME USERNAME";
            return response;
        }

        try {
            if (!string.IsNullOrEmpty(request.Status))
            {
                Status? foundStatus = await statusContext.Statuses.SingleOrDefaultAsync(row => row.Name == request.Status);
                statusId = foundStatus?.Id ?? 0;
                if (statusId <= 0)
                {
                    await statusContext.AddAsync(new Status { Name = request.Status });
                    await statusContext.SaveChangesAsync();
                    foundStatus = await statusContext.Statuses.SingleOrDefaultAsync(row => row.Name == request.Status);
                    statusId = foundStatus?.Id ?? 0; // this time the status if found, this is just to avoid compiler errors
                }
            }
        }
        catch
        {
            response.Status = -1;
            response.Message = "INCONSISTENT BASE - TWO STATUSES OR MORE WITH THE SAME STATUS NAME";
            return response;
        }
        #endregion

        DateTime now = DateTime.UtcNow;
        CriminalCode added = new CriminalCode();

        added.Name = string.IsNullOrEmpty(request.Name) ? added.Name : request.Name;
        added.Description = string.IsNullOrEmpty(request.Description) ? added.Description : request.Description;
        added.Penalty = request.Penalty == default(Decimal) ? added.Penalty : request.Penalty;
        added.PrisonTime = request.PrisonTime == default(int) || request.PrisonTime == 0 ? added.PrisonTime : request.PrisonTime;
        added.StatusId = statusId <= 0 ? added.StatusId : statusId;
        added.CreateUserId = userId;
        added.CreateDate = now;
        added.UpdateUserId = userId;
        added.UpdateDate = now;

        await criminalCodeContext.AddAsync(added);
        await criminalCodeContext.SaveChangesAsync();

        return response;
    }

    [Route("Exclude")]
    [HttpDelete]
    public async Task<ExcludeResponse> ExcludeAsync(ExcludeRequest request)
    {
        ExcludeResponse response = new ExcludeResponse();

        CriminalCodeContext criminalCodeContext = new CriminalCodeContext();

        if (request.CriminalCodeId <= 0 && string.IsNullOrEmpty(request.CriminalCodeName))
        {
            response.Status = -1;
            response.Message = "INVALID PARAMETERS";
            return response;
        }

        try {
            CriminalCode? found = await RetrieveCriminalCode(criminalCodeContext, request.CriminalCodeId, request.CriminalCodeName);

            if (found == null)
            {
                response.Status = -1;
                response.Message = "NOTHING WAS REMOVED";
                return response;
            }
            else {
                // context.Entry(found).State = EntityState.Deleted;
                // await context.SaveChangesAsync();
                await criminalCodeContext.SetStateAsync(found, EntityState.Deleted);
            }
        }
        catch
        {
            response.Status = -1;
            response.Message = "TOO MANY RESULTS";
            return response;
        }

        return response;
    }

    [Route("Edit")]
    [HttpPut]
    public async Task<EditResponse> EditAsync(EditRequest request)
    {
        EditResponse response = new EditResponse();

        CriminalCodeContext criminalCodeContext = new CriminalCodeContext();
        UserContext userContext = new UserContext();
        StatusContext statusContext = new StatusContext();

        string accessToken = "";

        if (!string.IsNullOrEmpty(request.AccessToken))
        {
            accessToken = request.AccessToken;
        }
        else if (!string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]))
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            accessToken = authorization.Replace("Bearer ", "");
        }
        else
        {
            // this case would not be found due middleware behavior, but we ensure nevertheless the handling
            response.Status = -1;
            response.Message = "UNEXPECTED BEHAVIOR";
            return response;
        }

        int userId = 0;
        int statusId = 0;
        string tokenUserName = JwtUtils.GetUserNameFromJwt(accessToken);

        #region Loading associated data (both user and status)
        try {
            User? foundUser = await userContext.Users.SingleOrDefaultAsync(row => row.UserName == tokenUserName);
            userId = foundUser?.Id ?? 0;

            if (userId <= 0)
            {
                response.Status = -1;
                response.Message = "INCONSISTENT BASE - USER WAS REMOVED FROM STORE WITH ACTIVE TOKEN";
                return response;
            }
        }
        catch
        {
            response.Status = -1;
            response.Message = "INCONSISTENT BASE - TWO USERS OR MORE WITH THE SAME USERNAME";
            return response;
        }

        try {
            if (!string.IsNullOrEmpty(request.Status))
            {
                Status? foundStatus = await statusContext.Statuses.SingleOrDefaultAsync(row => row.Name == request.Status);
                statusId = foundStatus?.Id ?? 0;
                if (statusId <= 0)
                {
                    await statusContext.AddAsync(new Status { Name = request.Status });
                    await statusContext.SaveChangesAsync();
                    foundStatus = await statusContext.Statuses.SingleOrDefaultAsync(row => row.Name == request.Status);
                    statusId = foundStatus?.Id ?? 0; // this time the status if found, this is just to avoid compiler errors
                }
            }
        }
        catch
        {
            response.Status = -1;
            response.Message = "INCONSISTENT BASE - TWO STATUSES OR MORE WITH THE SAME STATUS NAME";
            return response;
        }
        #endregion

        CriminalCode? found = await criminalCodeContext.CriminalCodes.SingleOrDefaultAsync(row => row.Id == request.Id);

        if (found == null)
        {
            response.Status = -1;
            response.Message = "COULD NOT FIND A CRIMINAL CODE TO UPDATE";
        }
        else {
            found.Name = string.IsNullOrEmpty(request.Name) ? found.Name : request.Name;
            found.Description = string.IsNullOrEmpty(request.Description) ? found.Description : request.Description;
            found.Penalty = request.Penalty == default(Decimal) ? found.Penalty : request.Penalty;
            found.PrisonTime = request.PrisonTime == default(int) || request.PrisonTime == 0 ? found.PrisonTime : request.PrisonTime;
            found.StatusId = statusId <= 0 ? found.StatusId : statusId;
            found.UpdateUserId = userId;
            found.UpdateDate = DateTime.UtcNow;

            await criminalCodeContext.SetStateAsync(found, EntityState.Modified);
        }

        return response;
    }

    [Route("Find")]
    [HttpGet]
    public async Task<FindResponse> FindAsync([FromQuery] FindRequest request)
    {
        FindResponse response = new FindResponse();
        CriminalCodeContext criminalCodeContext = new CriminalCodeContext();

        if (request.CriminalCodeId <= 0 && string.IsNullOrEmpty(request.CriminalCodeName))
        {
            response.Status = -1;
            response.Message = "INVALID PARAMETERS";
            return response;
        }

        try {
            CriminalCode? found = await RetrieveCriminalCode(criminalCodeContext, request.CriminalCodeId, request.CriminalCodeName);

            if (found == null)
            {
                response.Status = -1;
                response.Message = "NOT FOUND";
                return response;
            }
            else {
                response.FoundCriminalCode = found;
            }
        }
        catch
        {
            response.Status = -1;
            response.Message = "TOO MANY RESULTS";
            return response;
        }

        return response;
    }
}
