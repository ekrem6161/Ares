namespace Ares.Core;

public enum RolTipi { System, User, Assistant }

public record Mesaj(RolTipi Rol, string Icerik);
