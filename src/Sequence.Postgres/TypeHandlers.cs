using System;
using System.Data;
using Sequence.Core;
using static Dapper.SqlMapper;

namespace Sequence.Postgres
{
    internal sealed class GameIdTypeHandler : TypeHandler<GameId>
    {
        public override GameId Parse(object value) => new GameId((Guid)value);
        public override void SetValue(IDbDataParameter parameter, GameId value) => parameter.Value = Guid.Parse(value.ToString());
    }

    internal sealed class PlayerIdTypeHandler : StringTypeHandler<PlayerId>
    {
        protected override string Format(PlayerId xml) => xml.ToString();
        protected override PlayerId Parse(string xml) => new PlayerId(xml);
    }

    internal sealed class SeedTypeHandler : TypeHandler<Seed>
    {
        public override Seed Parse(object value) => new Seed((int)value);
        public override void SetValue(IDbDataParameter parameter, Seed value) => parameter.Value = value.ToInt32();
    }
}
