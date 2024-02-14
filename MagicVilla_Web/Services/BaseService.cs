using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public APIResponse responseModel { get; set; }

        public BaseService(IHttpClientFactory httpClientFactory)
        {
            responseModel = new();
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MagicAPI");

                var message = CreateRequestMessage(apiRequest);

                SetAuthorizationHeader(client, apiRequest.Token);

                var apiResponse = await client.SendAsync(message);
                var apiContent = await apiResponse.Content.ReadAsStringAsync();

                HandleApiResponse<T>(apiResponse, apiContent);

                var finalApiResponse = JsonConvert.DeserializeObject<T>(apiContent);
                return finalApiResponse;
            }
            catch (Exception e)
            {
                return HandleException<T>(e);
            }
        }

        private HttpRequestMessage CreateRequestMessage(APIRequest apiRequest)
        {
            var message = new HttpRequestMessage
            {
                Headers = { { "Accept", "application/json" } },
                RequestUri = new Uri(apiRequest.Url)
            };

            if (apiRequest.Data != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                    Encoding.UTF8, "application/json");
            }

            message.Method = apiRequest.ApiType switch
            {
                SD.ApiType.POST => HttpMethod.Post,
                SD.ApiType.PUT => HttpMethod.Put,
                SD.ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get
            };

            return message;
        }

        private void SetAuthorizationHeader(HttpClient client, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private void HandleApiResponse<T>(HttpResponseMessage apiResponse, string apiContent)
        {
            try
            {
                var apiResponseObject = JsonConvert.DeserializeObject<APIResponse>(apiContent);

                if (apiResponseObject != null && (apiResponse.StatusCode == System.Net.HttpStatusCode.BadRequest
                                                  || apiResponse.StatusCode == System.Net.HttpStatusCode.NotFound))
                {
                    apiResponseObject.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    apiResponseObject.IsSuccess = false;
                    var serializedResponse = JsonConvert.SerializeObject(apiResponseObject);
                    var returnObj = JsonConvert.DeserializeObject<T>(serializedResponse);
                    throw new Exception(JsonConvert.SerializeObject(returnObj));
                }
            }
            catch
            {
                var exceptionResponse = JsonConvert.DeserializeObject<T>(apiContent);
                throw new Exception(JsonConvert.SerializeObject(exceptionResponse));
            }
        }

        private T HandleException<T>(Exception e)
        {
            var dto = new APIResponse
            {
                ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                IsSuccess = false
            };

            var serializedDto = JsonConvert.SerializeObject(dto);
            var errorApiResponse = JsonConvert.DeserializeObject<T>(serializedDto);
            return errorApiResponse;
        }
    }
}