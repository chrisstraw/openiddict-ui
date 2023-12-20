using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace tomware.OpenIddict.UI.Suite.Api;

public static class RoutePrefixExtension
{
  public static void UseOpenIddictUIRoutePrefix(
    this MvcOptions opts,
    string prefix,
    IEnumerable<Type> controllerTypes
  )
  {
    opts.Conventions.Insert(0, new RoutePrefixConvention(
      new RouteAttribute(prefix),
      controllerTypes
    ));
  }
}

internal sealed class RoutePrefixConvention : IApplicationModelConvention
{
  private readonly AttributeRouteModel _routePrefix;
  private readonly IEnumerable<Type> _controllerTypes;

  public RoutePrefixConvention(IRouteTemplateProvider route, IEnumerable<Type> controllerTypes)
  {
    _routePrefix = new AttributeRouteModel(route);
    _controllerTypes = controllerTypes
      ?? throw new ArgumentNullException(nameof(controllerTypes));
  }

  public void Apply(ApplicationModel application)
  {
    var controllers = application.Controllers
      .Where(c =>
      {
        return _controllerTypes.Contains(c.ControllerType);
      });
    foreach (var controller in controllers)
    {
      var matchedSelectors = controller.Selectors
        .Where(x =>
        {
          return x.AttributeRouteModel != null;
        })
        .ToList();
      if (matchedSelectors.Count != 0)
      {
        foreach (var selectorModel in matchedSelectors)
        {
          selectorModel.AttributeRouteModel = AttributeRouteModel
            .CombineAttributeRouteModel(_routePrefix, selectorModel.AttributeRouteModel);
        }
      }

      var unmatchedSelectors = controller.Selectors
        .Where(x =>
        {
          return x.AttributeRouteModel == null;
        })
        .ToList();
      if (unmatchedSelectors.Count != 0)
      {
        foreach (var selectorModel in unmatchedSelectors)
        {
          selectorModel.AttributeRouteModel = _routePrefix;
        }
      }
    }
  }
}
