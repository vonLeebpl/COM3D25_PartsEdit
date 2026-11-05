using System;
using System.Collections.Generic;

namespace CM3D2.PartsEdit.Plugin.Db {
    // Klasy bazy sa celowo niezalezne od Unity i BepInEx (same liczby i napisy), zeby plik JSON nie zalezal
    // od tego, co jest akurat zaladowane w grze, i zeby dalo sie je testowac poza gra.

    // Jak wpis dopasowuje sie do przedmiotu zalozonego w slocie.
    public enum MatchMode {
        // do dowolnego przedmiotu w slocie
        AnyInSlot = 0,
        // tylko do przedmiotu o tej samej nazwie obiektu (ObjectName)
        SameObjectName = 1
    }

    public struct Vec3 {
        public float x, y, z;
        public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct Quat {
        public float x, y, z, w;
        public Quat(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    // Lokalna poza jednej kosci (lub korzenia obiektu).
    public class BoneRecord {
        public string Name { get; set; }
        public Vec3 Position { get; set; }
        public Quat Rotation { get; set; }
        public Vec3 Scale { get; set; }

        public BoneRecord() {
            Name = "";
            Position = new Vec3(0f, 0f, 0f);
            Rotation = new Quat(0f, 0f, 0f, 1f);
            Scale = new Vec3(1f, 1f, 1f);
        }
    }
}
