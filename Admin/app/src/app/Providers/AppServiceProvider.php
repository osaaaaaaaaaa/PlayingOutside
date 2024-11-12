<?php

namespace App\Providers;

use Illuminate\Support\ServiceProvider;
use Illuminate\Support\Facades\URL;
use Illuminate\Pagination\Paginator;

class AppServiceProvider extends ServiceProvider
{
    /**
     * Register any application services.
     */
    public function register(): void
    {
        //
    }

    /**
     * Bootstrap any application services.
     */
    public function boot(): void
    {
        // Azureコンテナを使用する場合、httpsを使用する
        if (config('app.env') === 'production') {
            URL::forceScheme('https');
        }
        // Bootstrapのものを使用する
        Paginator::useBootstrapFive();
    }
}
