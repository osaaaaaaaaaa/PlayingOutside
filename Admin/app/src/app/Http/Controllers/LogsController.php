<?php

namespace App\Http\Controllers;

use App\Models\User;
use Illuminate\Http\Request;
use Barryvdh\Debugbar\Facades\Debugbar;

class LogsController extends Controller
{
    // フォローログ一覧表示
    public function follow(Request $request)
    {
        // モデルを取得する
        $user = User::find($request->id);

        // ユーザーが存在するかどうか
        if (!empty($user)) {
            // リレーション
            $logs = $user->follow_logs()->paginate(10);
            $logs->appends(['id' => $request->id]);
        }

        return view('logs.follow', ['user' => $user, 'logs' => $logs ?? null]);
    }

    // アイテムログ一覧表示
    public function item(Request $request)
    {
        // モデルを取得する
        $user = User::find($request->id);

        // ユーザーが存在するかどうか
        if (!empty($user)) {
            // リレーション
            $logs = $user->item_logs()->paginate(10);
            $logs->appends(['id' => $request->id]);
        }

        return view('logs.item', ['user' => $user, 'logs' => $logs ?? null]);
    }

    // メールログ一覧表示
    public function mail(Request $request)
    {
        // モデルを取得する
        $user = User::find($request->id);

        // ユーザーが存在するかどうか
        if (!empty($user)) {
            // リレーション
            $logs = $user->mail_logs()->paginate(10);
            $logs->appends(['id' => $request->id]);
        }

        return view('logs.mail', ['user' => $user, 'logs' => $logs ?? null]);
    }

    // ステージリザルト情報
    public function stageresult(Request $request)
    {
        // モデルを取得する
        $user = User::find($request->id);

        // ユーザーが存在するかどうか
        if (!empty($user)) {
            // リレーション
            $logs = $user->stageresult()->paginate(10);
            $logs->appends(['id' => $request->id]);
        }

        return view('logs.stageresult', ['user' => $user, 'logs' => $logs ?? null]);
    }
}
