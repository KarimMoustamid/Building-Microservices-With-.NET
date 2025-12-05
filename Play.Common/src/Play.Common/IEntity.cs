namespace Play.Common.Repositories
{
    using System;

    public interface IEntity
    {
        Guid Id { get; set; }
    }
}