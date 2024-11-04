﻿using Domain.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.ModelBinders;

public class ListQueryParamsBinder : IModelBinder
{
    private ListQueryParams result;
    private ModelBindingContext context;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        result = new ListQueryParams();
        context = bindingContext ?? throw new Exception(nameof(bindingContext));

        /*
         * Фильтрация
         */
        // SetValue<string>(nameof(result.Search), null);
        // SetValue<int>(nameof(result.CompanyId), IntConverter);
        // SetValue<int>(nameof(result.LicenseId), IntConverter);
        // SetValue<int>(nameof(result.WorkplaceId), IntConverter);
        // SetValue<int>(nameof(result.WorkScheduleId), IntConverter);

        /*
        * Сортировка
        */
        SetValue<string>(nameof(result.SortBy), null);
        if (!SetValue<bool>(nameof(result.Desc), BoolConverter))
            result.Desc = false;

        /*
         * Пагинация
         */
        if (!SetValue<int>(nameof(result.Skip), IntConverter))
            result.Skip = 0;
        if (!SetValue<int>(nameof(result.Take), IntConverter))
            result.Take = 20;

        context.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }

    private bool SetValue<T>(string propName, TryConverter<T>? convert)
    {
        var valueProvider = context.ValueProvider.GetValue(propName);
        if (valueProvider == ValueProviderResult.None)
            return false; // в запросе не найден параметр с таким именем (регистронезависимый поиск)

        var rawValue = valueProvider.FirstValue;
        if (rawValue is null)
            return false; // нет смысла назначать null

        var propInfo = result.GetType().GetProperty(propName);
        if (convert is null)
        {
            propInfo.SetValue(result, rawValue);
            return true;
        }

        if (convert(rawValue, out var convertedValue)) // процесс конвертации завершился успехом
        {
            propInfo.SetValue(result, convertedValue);
            return true;
        }

        var message = $"Conversion error. Parsing list query param \"{propName}\"=\"{rawValue}\"";
        //TODO Логировать

        return false;
    }


    #region Конвертирование

    private delegate bool TryConverter<T>(string str, out T convertedValue);

    private static bool IntConverter(string str, out int convertedValue) => int.TryParse(str, out convertedValue);
    private static bool BoolConverter(string str, out bool convertedValue) => bool.TryParse(str, out convertedValue);

    #endregion Конвертирование
}