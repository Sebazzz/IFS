// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : EndpointRoutingExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace IFS.Web.Framework.Routing;

public static class EndpointRoutingExtensions
{
    public static IEndpointConventionBuilder WithName(this IEndpointConventionBuilder endpointBuilder,string endpointName)
    {
        return endpointBuilder.WithMetadata(
            new EndpointNameMetadata(endpointName),
            new RouteNameMetadata(endpointName)
        );
    }
}