using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using EchelonTouchInc.Gister.Api;
using EchelonTouchInc.Gister.Api.Credentials;
using EnvDTE;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;


namespace EchelonTouchInc.Gister
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidGisterPkgString)]
    public sealed class GisterPackage : Package
    {
        public GisterPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this));
        }

        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));

            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null == mcs) return;

            CommandID createGistCommand = new CommandID(GuidList.guidGisterCmdSet, (int)PkgCmdIDList.cmdCreateGist);
            MenuCommand createGistMenuItem = new MenuCommand(CreateGistCallback, createGistCommand);
            mcs.AddCommand(createGistMenuItem);

            CommandID createGistWithDescriptionCommand = new CommandID(GuidList.guidGisterCmdSet, (int)PkgCmdIDList.cmdCreateGistWithDescription);
            MenuCommand createGistWithDescriptionMenuItem = new MenuCommand(CreateGistWithDescriptionCallback, createGistWithDescriptionCommand);
            mcs.AddCommand(createGistWithDescriptionMenuItem);
        }
        private void CreateGistCallback(object sender, EventArgs e)
        {
            PostGist();

        }

        private void PostGist(string description="",bool ispublic=true)
        {
            IWpfTextView view = GetActiveTextView();

            if (NotReadyRockAndRoll(view)) return;

            string content = GetCurrentContentForGist(view);
            string fileName = GetCurrentFilenameForGist();

            GitHubCredentials credentials = GetGitHubCredentials();

            NotifyUserThat("Creating gist for {0}", fileName);

            if (credentials == GitHubCredentials.Anonymous)
            {
                NotifyUserThat("Cancelled Gist");
                return;
            }

            UploadsGists uploadsGists = new UploadsGists
            {
                GitHubSender = new HttpGitHubSender(),
                CredentialsAreBad = () =>
                {
                    NotifyUserThat("Gist not created.  Invalid GitHub Credentials");
                    new CachesGitHubCredentials().AssureNotCached();
                },
                Uploaded = url =>
                {
                    Clipboard.SetText(url);
                    new CachesGitHubCredentials().Cache(credentials);

                    NotifyUserThat("Gist created successfully.  Url placed in the clipboard.");
                }
            };

            uploadsGists.UseCredentials(credentials);

            uploadsGists.Upload(fileName, content, description, ispublic);
        }


        private void CreateGistWithDescriptionCallback(object sender, EventArgs e)
        {

            Func<IDescriptionPrompt> CreatePrompt = () => new GitHubDescriptionPrompt();

            IDescriptionPrompt prompt = CreatePrompt();
            prompt.Prompt();
            string description = prompt.Description;
            bool isPublic = prompt.GistPrivate == false ? true : false;
            PostGist(description,isPublic);
        }


        private static GitHubCredentials GetGitHubCredentials()
        {
            IRetrievesCredentials[] retrievers = new IRetrievesCredentials[]
                                 {
                                     new CachesGitHubCredentials(),
                                     new RetrievesUserEnteredCredentials()
                                 };

            IRetrievesCredentials firstAppropriate = (from applier in retrievers
                                    where applier.IsAvailable()
                                    select applier).First();

            return firstAppropriate.Retrieve();
        }

        private bool NotReadyRockAndRoll(IPropertyOwner view)
        {
            return view == null ||
                   Dte.ActiveDocument == null;
        }

        private void NotifyUserThat(string format, params object[] args)
        {
            IOleComponentUIManager uiManager = ((IOleComponentUIManager)GetService(typeof(SOleComponentUIManager)));

            if (uiManager == null) return;

            string message = string.Format(format, args);

            uiManager.SetStatus(message, UInt32.Parse("0"));
        }

        private string GetCurrentFilenameForGist()
        {
            return Dte.ActiveDocument.Name;
        }

        private DTE Dte
        {
            get { return (DTE)GetService(typeof(DTE)); }
        }

        private static string GetCurrentContentForGist(ITextView view)
        {
            if (SelectionIsAvailable(view))
                return GetSelectedText(view);

            return view.TextSnapshot.GetText();
        }

        private static string GetSelectedText(ITextView view)
        {
            return view.Selection.SelectedSpans[0].GetText();
        }

        private static bool SelectionIsAvailable(ITextView view)
        {
            if (view == null) throw new ArgumentNullException("view");

            return !view.Selection.IsEmpty && view.Selection.SelectedSpans.Count > 0;
        }

        private IWpfTextView GetActiveTextView()
        {
            IWpfTextView view = null;
            IVsTextView vTextView;

            IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            const int mustHaveFocus = 1;

            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);

            IVsUserData userData = vTextView as IVsUserData;
            if (null != userData)
            {
                object holder;

                Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
                userData.GetData(ref guidViewHost, out holder);

                IWpfTextViewHost viewHost = (IWpfTextViewHost)holder;
                view = viewHost.TextView;
            }

            return view;
        }
    }
}