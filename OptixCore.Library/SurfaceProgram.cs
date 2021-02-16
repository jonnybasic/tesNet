using System;

namespace OptixCore.Library
{
    /// <summary>
    /// A <see cref="OptixProgram">Program</see> representing a Surface shader program. Called when a ray intersects a surface.
    /// </summary>
    public class SurfaceProgram : OptixProgram
    {

        /// <summary>
        /// Creates a program located at [filename] with main function [programName]
        /// </summary>
        /// <param name="context">Context with which to create the program.</param>
        /// <param name="type">Type of ray this program represents. <seealso cref="RayHitType"></seealso></param>
        /// <param name="filename">Path of the compiled cuda ptx file holding the program.</param>
        /// <param name="programName">Name of the main function of the program.</param>
        public SurfaceProgram(Context context, RayHitType type, String filename, String programName) :base(context, filename, programName)
        {
            RayType = type;
        }

        internal SurfaceProgram(Context ctx, RayHitType type, IntPtr program):base(ctx, program)
        {
            RayType = type;
        }

        internal SurfaceProgram(Context ctx, RayHitType type) : base(ctx)
        {
            RayType = type;
        }

        public static SurfaceProgram CreateFromProgram(OptixProgram program, RayHitType type)
        {
            return new SurfaceProgram(program.mContext, type, program.InternalPtr);
        }

        public static SurfaceProgram CreateFromPTXString(Context context, RayHitType type, string ptx, string programName)
        {
            if (string.IsNullOrWhiteSpace(ptx))
            {
                throw new ArgumentException("A valid PTX String is required", "filename");
            }
            else if (string.IsNullOrWhiteSpace(programName))
            {
                throw new ArgumentException("A valid program name is required", "programName");
            }
            SurfaceProgram result = new SurfaceProgram(context, type);
            result.CheckError(Native.Api.rtProgramCreateFromPTXString(context.InternalPtr, ptx, programName, out result.InternalPtr));
            return result;
        }

        public static SurfaceProgram CreateFromPTXFile(Context context, RayHitType type, string filename, string programName)
        {
            if (string.IsNullOrWhiteSpace(filename) || System.IO.File.Exists(filename))
            {
                throw new ArgumentException("A valid PTX File is required", "filename");
            }
            else if (string.IsNullOrWhiteSpace(programName))
            {
                throw new ArgumentException("A valid program name is required", "programName");
            }
            SurfaceProgram result = new SurfaceProgram(context, type);
            result.CheckError(Native.Api.rtProgramCreateFromPTXFile(context.InternalPtr, filename, programName, out result.InternalPtr));
            return result;
        }

        public RayHitType RayType { get; }
    }
}