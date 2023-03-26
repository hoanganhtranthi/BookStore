using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using BookStore.Data.Extensions;
using BookStore.Data.Models;
using BookStore.Data.UnitOfWork;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using BookStore.Service.Helpers;
using BookStore.Service.Service.InterfaceService;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.ImplService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }
        public async Task<NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<UserResponse>> DeleteUser(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetAsync(u => u.UserId == id);
            try
            {
                if (id <= 0)
                {
                    throw new Exception();
                }
                _unitOfWork.Repository<User>().Delete(user);
                await _unitOfWork.CommitAsync();
                return new NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<UserResponse>()
                {
                    Status = new StatusViewModel
                    {
                        Success = true,
                        ErrorCode = 0,
                        Message = "sucess"
                    },
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Delete User Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<UserResponse>> GetUserByEmail(string email)
        {
            try
            {
                var cacheData = _cacheService.GetData<UserResponse>($"User{email}");
                if (cacheData == null)
                {

                    if (email.Equals(""))
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "Id User Invalid", "");
                    }
                    var response = await _unitOfWork.Repository<User>().GetAsync(u => u.Email == email);

                    var expiryTime = DateTimeOffset.Now.AddMinutes(2);
                    cacheData = _mapper.Map<UserResponse>(response);
                    _cacheService.SetData<UserResponse>($"User{email}", cacheData, expiryTime);

                    return new BaseResponseViewModel<UserResponse>()
                    {
                        Status = new StatusViewModel
                        {
                            ErrorCode = 0,
                            Message = "sucess",
                            Success = true
                        },
                        Data = cacheData
                    };
                }

                return new BaseResponseViewModel<UserResponse>()
                {
                    Status = new StatusViewModel
                    {
                        ErrorCode = 0,
                        Message = "sucess",
                        Success = true
                    },
                    Data = cacheData
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Get User By Email Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<UserResponse>> GetUserByID(int id)
        {
            try
            {
                var cacheData = _cacheService.GetData<UserResponse>($"User{id}");
                if (cacheData == null)
                {

                    if (id <= 0)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "Id User Invalid", "");
                    }
                    var response = await _unitOfWork.Repository<User>().GetAsync(u => u.UserId == id);

                    var expiryTime = DateTimeOffset.Now.AddMinutes(2);
                    cacheData = _mapper.Map<UserResponse>(response);
                    _cacheService.SetData<UserResponse>($"User{id}", cacheData, expiryTime);

                    return new BaseResponseViewModel<UserResponse>()
                    {
                        Status = new StatusViewModel
                        {
                            ErrorCode = 0,
                            Message = "sucess",
                            Success = true
                        },
                        Data = cacheData
                    };
                }

                return new BaseResponseViewModel<UserResponse>()
                {
                    Status = new StatusViewModel
                    {
                        ErrorCode = 0,
                        Message = "sucess",
                        Success = true
                    },
                    Data = cacheData
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Get User By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<BasePagingViewModel<UserResponse>> GetUsers(PagingRequest pagingRequest, UserRequest userRequest)
        {
            try
            {
                var filter = _mapper.Map<UserResponse>(userRequest);
                filter.SortDirection = pagingRequest.SortDirection;
                filter.SortProperty = pagingRequest.SortProperty;

                var rsFilter = _unitOfWork.Repository<User>().GetAll()
                                .ProjectTo<UserResponse>(_mapper.ConfigurationProvider)
                                .Where(a => a.DateOfBirth >= filter.DateOfBirth)
                                .DynamicSort(filter).DynamicFilter(filter)
                                .PagingQueryable(pagingRequest.Page, pagingRequest.PageSize).Item2;

                return new BasePagingViewModel<UserResponse>()
                {
                    Metadata = pagingRequest,
                    Data = rsFilter.ToList()
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Get Users Paging Error!!!", ex.InnerException?.Message);
            }

        }

        public async Task<BaseResponseViewModel<UserResponse>> PutUser(int id, UserRequest model)
        {
            var user = await _unitOfWork.Repository<User>().GetAsync(u => u.UserId == id);
            try
            {
                if (user == null)
                {
                    throw new Exception();
                }
                var response = _mapper.Map<UserRequest, User>(model, user);
                await _unitOfWork.Repository<User>().Update(response, id);
                await _unitOfWork.CommitAsync();
                return new NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<UserResponse>()
                {
                    Status = new StatusViewModel
                    {
                        ErrorCode = 0,
                        Message = "sucess",
                        Success = true
                    },
                    Data = _mapper.Map<UserResponse>(user)
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Update User Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
