/// <summary>
/// Проджектайл с эффектами (яд, огонь и т.п.).
/// Отдельный тип нужен, чтобы PoolManager хранил его в отдельном пуле от обычного AiProjectile.
/// </summary>
public class EffectedProjectile : AiProjectile { }
