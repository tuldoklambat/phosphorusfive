/*
 * Phosphorus Five, copyright 2014 - 2017, Thomas Hansen, thomas@gaiasoul.com
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

using System.IO;
using System.Linq;
using System.Security;
using DevOne.Security.Cryptography.BCrypt;
using p5.exp;
using p5.core;
using p5.exp.exceptions;

namespace p5.auth.helpers
{
    /// <summary>
    ///     Class wrapping users features of Phosphorus Five.
    /// </summary>
    static class Users
    {
        /*
         * Lists all users in system.
         */
        public static void ListUsers (ApplicationContext context, Node args)
        {
            // Retrieving "auth" file in node format.
            var authFile = AuthFile.GetAuthFile (context);
            
            // Retrieving guest account name, to make sure we exclude it as a user, since it's not a "real user" per se.
            var guestAccountName = context.RaiseEvent (".p5.auth.get-default-context-username").Get<string> (context);

            // Looping through each user in [users] node of "auth" file.
            foreach (var idxUserNode in authFile ["users"].Children) {

                // Ignoring "guest" account.
                if (idxUserNode.Name == guestAccountName)
                    continue;

                // Returning user's name, and role he belongs to.
                args.Add (idxUserNode.Name, idxUserNode ["role"].Value);
            }
        }
            
        /*
         * Creates a new user.
         */
        public static void CreateUser (ApplicationContext context, Node args)
        {
            // Retrieving arguments.
            var username = args.GetExValue<string> (context);
            var password = args.GetExChildValue<string> ("password", context);
            var role = args.GetExChildValue<string> ("role", context);
            
            // Sanity checking role name towards guest account name.
            if (role == context.RaiseEvent (".p5.auth.get-default-context-role").Get<string> (context))
                throw new LambdaException ("Sorry, but that's the name of our guest account role.", args, context);
            
            // Sanity checking username towards guest account name.
            if (username == context.RaiseEvent (".p5.auth.get-default-context-username").Get<string> (context))
                throw new LambdaException ("Sorry, but that's the name of our guest account.", args, context);

            // Making sure [password] never leaves method in case of an exception.
            args.FindOrInsert ("password").Value = "xxx";
            
            // Retrieving password rules from web.config, if any.
            if (!Passwords.IsGoodPassword (context, password)) {

                // New password was not accepted, throwing an exception.
                throw new LambdaSecurityException (
                    "Password didn't obey by your configuration settings, which are as follows; " +
                    Passwords.PasswordRuleDescription (context),
                    args,
                    context);
            }

            // Basic sanity check new user's data.
            if (string.IsNullOrEmpty (username) || string.IsNullOrEmpty (password) || string.IsNullOrEmpty (role))
                throw new LambdaException (
                    "User must have username as value, [password] and [role] at the very least",
                    args,
                    context);

            // Verifying username is valid, since we'll need to create a folder for user.
            VerifyUsernameValid (username);

            // To reduce lock time of "auth" file, we execute Blow Fish hashing before we enter lock.
            password = Passwords.SaltAndHashPassword (context, password);

            // Locking access to password file as we create new user object.
            AuthFile.ModifyAuthFile (
                context,
                delegate (Node authFile) {

                    // Checking if user exist from before.
                    if (authFile ["users"] [username] != null)
                        throw new LambdaException (
                            "Sorry, that [username] is already taken by another user in the system",
                            args,
                            context);

                    // Adding user.
                    var userNode = authFile ["users"].Add (username).LastChild;
                    
                    // Salting and hashing password, before storing it in "auth" file.
                    userNode.Add ("password", password);
                    
                    // Adding user to specified role.
                    userNode.Add ("role", role);

                    // Adding all other specified objects to user.
                    userNode.AddRange (args.Children.Where (ix => ix.Name != "password" && ix.Name != "role").Select (ix => ix.Clone ()));
                });

            // Creating newly created user's directory structure.
            CreateUserDirectory (context, username);
        }

        /*
         * Retrieves one or more specific users from the system.
         */
        public static void GetUser (ApplicationContext context, Node args)
        {
            // Retrieving "auth" file in node format.
            var authFile = AuthFile.GetAuthFile (context);

            // Iterating all users requested by caller.
            foreach (var idxUsername in XUtil.Iterate<string> (context, args)) {

                // Checking if user exist.
                if (authFile ["users"] [idxUsername] == null)
                    throw new LambdaException (
                        string.Format ("User '{0}' does not exist", idxUsername),
                        args,
                        context);

                // Adding user's node as return value, and each property of user, except [password].
                args.Add (idxUsername);
                args [idxUsername].AddRange (authFile ["users"] [idxUsername].Clone ().Children.Where (ix => ix.Name != "password"));
            }
        }

        /*
         * Retrieves a specific user from system.
         */
        public static void DeleteUser (ApplicationContext context, Node args)
        {
            // Locking access to password file as we create new user object.
            AuthFile.ModifyAuthFile (
                context,
                delegate (Node authFile) {

                    // Iterating all users requested deleted by caller.
                    foreach (var idxUsername in XUtil.Iterate<string> (context, args)) {

                        // Checking if user exist.
                        if (authFile ["users"] [idxUsername] == null)
                            throw new LambdaException (
                                string.Format ("User '{0}' does not exist", idxUsername),
                                args,
                                context);

                        // Deleting currently iterated user.
                        authFile ["users"] [idxUsername].UnTie ();

                        // Deleting user's home directory.
                        context.RaiseEvent ("p5.io.folder.delete", new Node ("", "/users/" + idxUsername + "/"));
                    }
                });
        }

        /*
         * Edits an existing user.
         */
        public static void EditUser (ApplicationContext context, Node args)
        {
            // Retrieving username, and sanity checking invocation.
            var username = args.GetExValue<string> (context);
            if (args ["username"] != null)
                throw new LambdaSecurityException ("Cannot change username for user", args, context);

            // Retrieving new password and role, defaulting to null, which will not update existing values.
            var password = args.GetExChildValue<string> ("password", context, null);
            var role = args.GetExChildValue<string> ("role", context, null);

            // Sanity checking role name towards guest account name.
            if (role == context.RaiseEvent (".p5.auth.get-default-context-role").Get<string> (context))
                throw new LambdaException ("Sorry, but that's the name of your system's guest account role.", args, context);

            // Changing user's password, but only if a [password] argument was explicitly supplied by caller.
            if (!string.IsNullOrEmpty (password)) {
                
                // Verifying password conforms to password rules.
                if (!Passwords.IsGoodPassword (context, password)) {

                    // New password was not accepted, throwing an exception.
                    args.FindOrInsert ("password").Value = "xxx";
                    var description = Passwords.PasswordRuleDescription (context);
                    throw new LambdaSecurityException ("Password didn't obey by your configuration settings, which are as follows; " + description, args, context);
                }
            }

            // To reduce lock time of "auth" file we execute Blow Fish hashing before we enter lock.
            password = password == null ? null : Passwords.SaltAndHashPassword (context, password);

            // Locking access to password file as we edit user object.
            AuthFile.ModifyAuthFile (
                context,
                delegate (Node authFile) {

                    // Checking to see if user exist.
                    if (authFile ["users"] [username] == null)
                        throw new LambdaException (
                            "Sorry, that user does not exist",
                            args,
                            context);
                    
                    // Updating user's password, but only if a new password was supplied by caller.
                    if (!string.IsNullOrEmpty (password))
                        authFile ["users"] [username] ["password"].Value = password;

                    // Updating user's role, if a new role was supplied by caller.
                    if (role != null)
                        authFile ["users"] [username] ["role"].Value = role;

                    // Checking if caller wants to edit settings.
                    if (args.Name == "p5.auth.users.edit") {

                        // Removing old settings.
                        authFile ["users"] [username].RemoveAll (ix => ix.Name != "password" && ix.Name != "role" && ix.Name != "salt");

                        // Adding all other specified objects to user.
                        foreach (var idxNode in args.Children.Where (ix => ix.Name != "password" && ix.Name != "role" && ix.Name != "salt")) {

                            authFile ["users"] [username].Add (idxNode.Clone ());
                        }
                    }
                });
        }

        #region [ -- Private helper methods -- ]

        /*
         * Verifies that given username is valid.
         */
        static void VerifyUsernameValid (string username)
        {
            foreach (var charIdx in username) {
                if ("abcdefghijklmnopqrstuvwxyz1234567890_-".IndexOf (charIdx) == -1)
                    throw new SecurityException ("Sorry, you cannot use the character '" + charIdx + "' in your usernames");
            }
        }

        /*
         * Creates folder structure for user
         */
        static void CreateUserDirectory (ApplicationContext context, string username)
        {
            // Retrieving root folder of system.
            var rootFolder = context.RaiseEvent (".p5.core.application-folder").Get<string> (context);

            // Creating folders for user, and making sure private directory stays private ...
            if (!Directory.Exists (rootFolder + "/users/" + username))
                Directory.CreateDirectory (rootFolder + "/users/" + username);

            if (!Directory.Exists (rootFolder + "/users/" + username + "/documents"))
                Directory.CreateDirectory (rootFolder + "/users/" + username + "/documents");

            if (!Directory.Exists (rootFolder + "/users/" + username + "/documents/private"))
                Directory.CreateDirectory (rootFolder + "/users/" + username + "/documents/private");

            if (!Directory.Exists (rootFolder + "/users/" + username + "/documents/public"))
                Directory.CreateDirectory (rootFolder + "/users/" + username + "/documents/public");

            if (!Directory.Exists (rootFolder + "/users/" + username + "/temp"))
                Directory.CreateDirectory (rootFolder + "/users/" + username + "/temp");
        }

        #endregion
    }
}