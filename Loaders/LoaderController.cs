using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Wintellect.PowerCollections;

using Utility;
using Builders;

namespace Loaders
{
    public class LoaderController
    {
        #region Fields
        Factory<ILoader> loaderFactory = null;
        #endregion

        #region Singleton

        private static readonly LoaderController instance = new LoaderController();

        public static LoaderController Instance
        {
            get { return instance; }
        }

        private LoaderController()
        {
            this.loaderFactory = Factory<ILoader>.Instance;
        }

        #endregion

        public void Open(Uri uri, IGeoModel model, string key)
        {
            bool finished = true;
            do
            {
                ILoader loader = loaderFactory.Create(key);
                finished = OpenWithFailover(loader, model, finished, false);
            }
            while (!finished);
        }

        public void Open(Uri uri, IGeoModel model)
        {
            string localPath = uri.LocalPath;
            string uriBasename = Path.GetFileName(localPath);

            // TODO: Some basic checks such as existence of the file
            //       should be done here.

            List<object> rankedKeys = new List<object>();
            foreach (object key in loaderFactory.Keys)
            {
                ILoader loader = loaderFactory.Create(key);
                loader.Location = uri;
                bool matched = false;
                foreach (Regex pattern in loader.UriPatterns)
                {
                    if (pattern.IsMatch(uriBasename))
                    {
                        matched = true;
                        break;
                    }
                }
                if (matched)
                {
                    rankedKeys.Insert(0, key);
                }
                else
                {
                    rankedKeys.Add(key);
                }
            }

            bool finished = false;
            List<object> maybeKeys = new List<object>();
            foreach (string key in rankedKeys)
            {
                ILoader loader = loaderFactory.Create(key);
                loader.Location = uri;
                // TODO: Wrap this is a try..catch and deal with failures such
                //       as file not found.
                Recognition recognition = loader.Recognize();
                if (recognition == Recognition.Yes)
                {
                    finished = OpenWithFailover(loader, model, finished, true);
                    if (finished)
                    {
                        break;
                    }
                    maybeKeys.Add(key);
                }
                else if (recognition == Recognition.Maybe)
                {
                    maybeKeys.Add(key);
                }
            }

            if (!finished)
            {
                if (maybeKeys.Count == 1)
                {
                    ILoader loader = loaderFactory.Create(maybeKeys[0]);
                    loader.Location = uri;
                    finished = OpenWithFailover(loader, model, finished, false);
                }
                else
                {
                    do
                    {
                        // Present the user with a choice
                        List<Pair<object, string>> choices = new List<Pair<object, string>>();
                        foreach (object key in maybeKeys)
                        {
                            ILoader loader = loaderFactory.Create(key);
                            loader.Location = uri;
                            choices.Add(new Pair<object, string>(key, loader.Description));
                        }
    
                        IChoiceList choiceList = Factory<IChoiceList>.Instance.CreateDefault(choices);
                        object chosenKey = choiceList.Show();
                        if (chosenKey != null)
                        {
                            ILoader loader = loaderFactory.Create(chosenKey);
                            loader.Location = uri;
                            finished = OpenWithFailover(loader, model, finished, true);
                        }
                        else
                        {
                            finished = true;
                        }
                    }
                    while (!finished);
                }
            }
        }

        enum OpenChoice
        {
            TryAgain,
            TryOthers,
            Abort
        }

        private static bool OpenWithFailover(ILoader loader, IGeoModel model, bool finished, bool showTryOthers)
        {
            // TODO: Maybe we can use the finished parameter to determine whether
            //       we need to present the 'Try Others' option. Maybe the finished
            //       arg should be renamed to hideTryOthers...
            bool tryAgain = false;
            do
            {
                try
                {
                    loader.Open(model);
                    finished = true;
                    break;
                }
                catch (OpenException openException)
                {
                    // TODO: Notify the user of the open failure
                    //       "Failed to open URI because ...
                    Console.WriteLine(openException.Message);
                    //        Do you want to Try again? Abort? Try other loaders?
                    List<Pair<object, string>> choices = new List<Pair<object, string>>(3);
                    choices.Add(new Pair<object, string>(OpenChoice.TryAgain, "Try Again"));
                    choices.Add(new Pair<object, string>(OpenChoice.TryOthers, "Try Other Formats"));
                    choices.Add(new Pair<object, string>(OpenChoice.Abort, "Cancel"));

                    IChoiceList choiceList = Factory<IChoiceList>.Instance.CreateDefault(choices);
                    OpenChoice decision = (OpenChoice) choiceList.Show();
                    switch (decision)
                    {
                        case OpenChoice.TryAgain:
                            tryAgain = true;
                            finished = false;
                            break;
                        case OpenChoice.TryOthers:
                            tryAgain = false;
                            finished = false;
                            break;
                        case OpenChoice.Abort:
                            tryAgain = false;
                            finished = true;
                            break;
                    }
                }
            }
            while (tryAgain);
            return finished;
        }
    }
}

