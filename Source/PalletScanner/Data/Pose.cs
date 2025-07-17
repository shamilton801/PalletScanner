using System.Numerics;

namespace PalletScanner.Data
{
    /// <summary>
    /// <para>
    /// Represents a rigid transformation from one coordinate system to another. <b>NOTE: By default, unless otherwise
    /// stated, the <c>Pose</c> of an object transforms its local space to the ambient world space. </b>
    /// </para>
    /// <para>
    /// Given two <c>Pose</c>s <c>p1</c> and <c>p2</c> which transform from space A to B and from B to C respectively,
    /// the pose <c>p2 * p1</c> transforms from A to C, and <c>p1 * v</c> transforms a vector <c>v</c> from space A to
    /// B. A <c>Pose</c> has a rotation and a translation. The rotation is always applied first, followed by the
    /// translation.
    /// </para>
    /// </summary>
    /// <param name="rotation">The rotation of this pose.</param>
    /// <param name="translation">The translation of this pose.</param>
    public struct Pose(Quaternion rotation, Vector3 translation)
    {
        public Vector3 Translation { readonly get; set; } = translation;
        public Quaternion Rotation { readonly get; set; } = rotation;

        public static Pose Identity => new(Quaternion.Identity, Vector3.Zero);

        public Pose(Vector3 translation) : this(Quaternion.Identity, translation) { }
        public Pose(Quaternion rotation) : this(rotation, Vector3.Zero) { }

        public static Vector3 operator *(Pose p, Vector3 v) => Vector3.Transform(v, p.Rotation) + p.Translation;
        public static Pose operator *(Pose a, Pose b) => new(a.Rotation * b.Rotation, a * b.Translation);
        public readonly Pose Inverse
        {
            get
            {
                var rotInv = Quaternion.Conjugate(Rotation);
                return new(rotInv, -Vector3.Transform(Translation, rotInv));
            }
        }
    }
}