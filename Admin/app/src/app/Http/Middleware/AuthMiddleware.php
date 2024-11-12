<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;

class AuthMiddleware
{
    public function handle(Request $request, Closure $next)
    {
        // ログインセッションがない場合->ログインページ表示
        if (!$request->session()->has('login')) {
            return redirect()->route('auths.index');
        }

        // 左辺:view表示 , 右辺:コントローラー実行
        $response = $next($request);
        return $response;
    }
}
