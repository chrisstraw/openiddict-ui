using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using tomware.OpenIddict.UI.Suite.Core;

namespace tomware.OpenIddict.UI.Infrastructure;

public class ScopeService : IScopeService
{
  private readonly IScopeRepository _repository;
  private readonly OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> _manager;

  public ScopeService(
    IScopeRepository repository,
    OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> manager
  )
  {
    _repository = repository
      ?? throw new ArgumentNullException(nameof(repository));
    _manager = manager
      ?? throw new ArgumentNullException(nameof(manager));
  }

  public async Task<IEnumerable<ScopeInfo>> GetScopesAsync()
  {
    var items = await _repository.ListAsync(new AllScopes());

    return items.Select(ToInfo);
  }

  public async Task<ScopeInfo> GetAsync(string id)
  {
    ArgumentNullException.ThrowIfNull(id);

    var entity = await _manager.FindByIdAsync(id);

    return entity != null ? ToInfo(entity) : null;
  }

  public async Task<string> CreateAsync(ScopeParam model)
  {
    ArgumentNullException.ThrowIfNull(model);

    var entity = await _manager.FindByNameAsync(model.Name);
    if (entity == null)
    {
      // create new entity
      var newEntity = new OpenIddictEntityFrameworkCoreScope
      {
        Name = model.Name,
        DisplayName = model.DisplayName,
        Description = model.Description,
      };

      HandleCustomProperties(model, newEntity);

      await _manager.CreateAsync(newEntity);

      return newEntity.Id;
    }

    // update existing entity
    model.Id = entity.Id;
    await UpdateAsync(model);

    return entity.Id;
  }

  public async Task UpdateAsync(ScopeParam model)
  {
    if (string.IsNullOrWhiteSpace(model.Id))
    {
      throw new InvalidOperationException(nameof(model.Id));
    }

    var entity = await _manager.FindByIdAsync(model.Id);

    SimpleMapper.Map(model, entity);

    HandleCustomProperties(model, entity);

    await _manager.UpdateAsync(entity);
  }

  public async Task DeleteAsync(string id)
  {
    ArgumentNullException.ThrowIfNull(id);

    var entity = await _manager.FindByIdAsync(id);

    await _manager.DeleteAsync(entity);
  }

  private static ScopeInfo ToInfo(OpenIddictEntityFrameworkCoreScope entity)
  {
    var info = SimpleMapper
      .From<OpenIddictEntityFrameworkCoreScope, ScopeInfo>(entity);

    info.Resources = entity.Resources != null
      ? JsonSerializer.Deserialize<List<string>>(entity.Resources)
      : [];

    return info;
  }

  private static void HandleCustomProperties(
    ScopeParam model,
    OpenIddictEntityFrameworkCoreScope entity
  ) => entity.Resources = JsonSerializer.Serialize(model.Resources);
}
