using Voltiq.Application.Features.Materials;
using Voltiq.Application.Features.Materials.Commands.RegisterMaterial;
using Voltiq.Application.Features.Materials.Commands.UpdateMaterial;
using Voltiq.Domain.Entities;

namespace Voltiq.Application.Mappings.Materials;

public static class MaterialMappingExtensions
{
    extension(RegisterMaterialRequest request)
    {
        public RegisterMaterialCommand ToCommand() =>
            new(request.Name, request.DefaultPrice, request.Unit);
    }

    extension(UpdateMaterialRequest request)
    {
        public UpdateMaterialCommand ToCommand(Guid id) =>
            new(id, request.Name, request.DefaultPrice, request.Unit);
    }

    extension(Material material)
    {
        public MaterialResponse ToResponse() =>
            new(material.Id, material.Name, material.DefaultPrice, material.Unit, material.IsActive);
    }
}
