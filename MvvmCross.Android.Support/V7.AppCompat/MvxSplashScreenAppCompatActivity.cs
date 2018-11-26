﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross.Core;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.ViewModels;

namespace MvvmCross.Droid.Support.V7.AppCompat
{
    [Register("mvvmcross.droid.support.v7.appcompat." + nameof(MvxSplashScreenAppCompatActivity))]
    public abstract class MvxSplashScreenAppCompatActivity
        : MvxAppCompatActivity, IMvxSetupMonitor
    {
        protected const int NoContent = 0;

        private readonly int _resourceId;

        private Bundle _bundle;

        public new MvxNullViewModel ViewModel
        {
            get { return base.ViewModel as MvxNullViewModel; }
            set { base.ViewModel = value; }
        }

        protected MvxSplashScreenAppCompatActivity(int resourceId = NoContent)
        {
            RegisterSetup();
            _resourceId = resourceId;
        }

        protected virtual void RequestWindowFeatures()
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
        }

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeatures();

            _bundle = bundle;

            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setup.InitializeAndMonitor(this);

            base.OnCreate(bundle);

            if (_resourceId != NoContent)
            {
                // Set our view from the "splash" layout resource
                // Be careful to use non-binding inflation
                var content = LayoutInflater.Inflate(_resourceId, null);
                SetContentView(content);
            }
        }

        private bool _isResumed;

        protected override void OnResume()
        {
            base.OnResume();
            _isResumed = true;
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setup.InitializeAndMonitor(this);
        }

        protected override void OnPause()
        {
            _isResumed = false;
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setup.CancelMonitor(this);
            base.OnPause();
        }

        public virtual async Task InitializationComplete()
        {
            if (!_isResumed)
                return;

            await RunAppStartAsync(_bundle);
        }

        protected virtual async Task RunAppStartAsync(Bundle bundle)
        {
            if (Mvx.IoCProvider.TryResolve(out IMvxAppStart startup))
            {
                if (!startup.IsStarted)
                    await startup.StartAsync(GetAppStartHint(bundle));
            }
        }

        protected virtual object GetAppStartHint(object hint = null)
        {
            return null;
        }

        protected virtual void RegisterSetup()
        {
        }
    }

    public abstract class MvxSplashScreenAppCompatActivity<TMvxAndroidSetup, TApplication> : MvxSplashScreenAppCompatActivity
            where TMvxAndroidSetup : MvxAppCompatSetup<TApplication>, new()
            where TApplication : class, IMvxApplication, new()
    {
        protected MvxSplashScreenAppCompatActivity(int resourceId = NoContent) : base(resourceId)
        {
        }

        protected override void RegisterSetup()
        {
            this.RegisterSetupType<TMvxAndroidSetup>();
        }
    }
}
