/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, mr.gaia@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using p5.exp;
using p5.core;
using p5.exp.exceptions;

namespace p5.io.common
{
    /// <summary>
    ///     Class to help copy, rename and/or move files
    /// </summary>
    public static class MoveCopyHelper
    {
        /// Delegate for actual implementation for handling one single file
        public delegate void MoveCopyDelegate (string rootFolder, string source, string destination);

        /// Delegate for checking if fileobject exists
        public delegate bool ObjectExistDelegate (string destination);

        /// <summary>
        ///     Common helper for iterating files/folders for move/copy/rename operation
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="args">Root node for Active Event invoking method</param>
        /// <param name="sourceAuthorizeEvent">Authorize event for source file(s)</param>
        /// <param name="destinationAuthorizeEvent">Authorize event for destination file(s)</param>
        /// <param name="functorMoveObject">Actual implementation of moving/copying a single file object</param>
        /// <param name="functorObjectExist">Expected to return true if object exist from before</param>
        public static void CopyMoveFileObject (
            ApplicationContext context, 
            Node args, 
            string sourceAuthorizeEvent,
            string destinationAuthorizeEvent,
            MoveCopyDelegate functorMoveObject, 
            ObjectExistDelegate functorObjectExist)
        {
            // Making sure we clean up and remove all arguments passed in after execution
            using (new Utilities.ArgsRemover (args)) {

                // Getting root folder
                var rootFolder = Common.GetRootFolder (context);

                // Checking if we're given an expression as source, to make sure we support Active Event source
                if (XUtil.IsExpression (args.Value)) {

                    // Destination is an expression, which means we might have an Active Event invocation source
                    var destEx = args.Value as Expression;
                    foreach (var idxSource in destEx.Evaluate (context, args, args)) {

                        // Retrieving destination, possibly relative to currently iterated expression result
                        var dest = XUtil.Source (context, args, idxSource.Node, "dest");

                        // Making sure source yields only one value
                        if (dest.Count != 1)
                            throw new LambdaException ("The destination for your [move-file] operation must be one string", args, context);
                        CopyMoveFileObjectImplementation (
                            context, 
                            args, 
                            rootFolder, 
                            Common.GetSystemPath (context, Utilities.Convert<string> (context, idxSource.Value)),
                            Common.GetSystemPath (context, Utilities.Convert<string> (context, dest[0])),
                            sourceAuthorizeEvent,
                            destinationAuthorizeEvent,
                            functorMoveObject,
                            functorObjectExist);
                    }
                } else {

                    // Source is a constant
                    // Retrieving destination
                    var dest = XUtil.Source (context, args, args, "dest");

                    // Making sure source yields only one value
                    if (dest.Count != 1)
                        throw new LambdaException ("The destination for your [move-file] operation must be one string", args, context);
                    CopyMoveFileObjectImplementation (
                        context,
                        args,
                        rootFolder,
                        Common.GetSystemPath (context, Utilities.Convert<string> (context, XUtil.FormatNode (context, args))),
                        Common.GetSystemPath (context, Utilities.Convert<string> (context, dest[0])),
                        sourceAuthorizeEvent,
                        destinationAuthorizeEvent,
                        functorMoveObject,
                        functorObjectExist);
                }
            }
        }

        /*
         * Helper for above
         */
        private static void CopyMoveFileObjectImplementation (
            ApplicationContext context, 
            Node args, 
            string rootFolder, 
            string sourceFile, 
            string destinationFile,
            string authorizeSourceEventName,
            string authorizeDestinationEventName,
            MoveCopyDelegate functorMoveCopy,
            ObjectExistDelegate functorObjectExist)
        {
            // Making sure we have a file, and not a folder for our destination.
            if (destinationFile.EndsWith ("/"))
                destinationFile += sourceFile.Substring (sourceFile.LastIndexOf ("/") + 1);

            // Making sure user is authorized to do file/folder operation
            Common.RaiseAuthorizeEvent (context, args, authorizeSourceEventName, sourceFile);
            Common.RaiseAuthorizeEvent (context, args, authorizeDestinationEventName, destinationFile);

            // Getting new filename of file, if needed
            if (functorObjectExist (rootFolder + destinationFile)) {

                // Destination file exist from before, throwing an exception
                throw new LambdaException (destinationFile + " already exist from before", args, context);
            }

            functorMoveCopy (rootFolder, sourceFile, destinationFile);
        }
    }
}
