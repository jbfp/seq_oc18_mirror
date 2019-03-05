using System;
using System.Data;
using static Dapper.SqlMapper;

namespace Sequence.Postgres
{
    internal sealed class GameIdTypeHandler : TypeHandler<GameId>
    {
        public override GameId Parse(object value) => new GameId((Guid)value);
        public override void SetValue(IDbDataParameter parameter, GameId value) => parameter.Value = Guid.Parse(value.ToString());
    }

    internal sealed class PlayerHandleTypeHandler : StringTypeHandler<PlayerHandle>
    {
        protected override string Format(PlayerHandle xml) => xml.ToString();
        protected override PlayerHandle Parse(string xml) => new PlayerHandle(xml);
    }

    internal sealed class PlayerIdTypeHandler : TypeHandler<PlayerId>
    {
        public override PlayerId Parse(object value) => new PlayerId((int)value);
        public override void SetValue(IDbDataParameter parameter, PlayerId value) => parameter.Value = value.ToInt32();
    }

    internal sealed class SeedTypeHandler : TypeHandler<Seed>
    {
        public override Seed Parse(object value) => new Seed((int)value);
        public override void SetValue(IDbDataParameter parameter, Seed value) => parameter.Value = value.ToInt32();
    }
}
