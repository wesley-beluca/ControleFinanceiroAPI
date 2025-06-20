using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ControleFinanceiro.Domain.Entities
{
    public class Result<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public T Data { get; private set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Errors { get; private set; }

        private Result()
        {
            Success = false;
            Message = string.Empty;
            Errors = new List<string>();
        }

        public static Result<T> Ok(T data, string message = "Operação realizada com sucesso")
        {
            return new Result<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static Result<T> Fail(string message)
        {
            return new Result<T>
            {
                Success = false,
                Message = message
            };
        }

        public static Result<T> Fail(List<string> errors)
        {
            return new Result<T>
            {
                Success = false,
                Message = "Ocorreram erros durante a operação",
                Errors = errors
            };
        }

        public static Result<T> Fail(string message, List<string> errors)
        {
            return new Result<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
} 