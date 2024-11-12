<?php

namespace App\Http\Controllers;

use App\Models\Account;
use Barryvdh\Debugbar\Facades\Debugbar;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Facades\Validator;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;

class AuthController extends Controller
{
    // ログイン画面を表示する
    public function index(Request $request)
    {
        // 既にログインしている場合
        if ($request->session()->has('login')) {
            return redirect()->route('home.index');
        }

        return view('auths/index', ['error' => $request['error'] ?? null]); // $request['error']がnullの場合は右辺の値(null)を入れる
    }

    // ログイン処理
    public function doLogin(Request $request)
    {
        // カスタムバリデーション
        $validator = Validator::make($request->all(), [
            'name' => ['required', 'min:4'],
            'password' => 'required',
        ]);

        if ($validator->fails()) {
            return redirect()->route('auths.index')
                ->withErrors($validator)
                ->withInput();
        }

        // レコードを取得
        $account = Account::where('name', '=', $request['name'])->get();
        if ($account->count() > 0) {// レコードを取得できた場合
            // ハッシュ化したkeyと一致するかどうか
            if (Hash::check($request['password'], $account[0]->password)) {
                // セッションに指定のキーで値を保存する
                $request->session()->put('login', true);
                $request->session()->put('login_id', $account[0]->id);
                return redirect()->route('home.index');
            }
        }
        // ルートに名前を指定
        return redirect()->route('auths.index', ['error' => 'invalid']);
    }

    // ログアウト処理
    public function doLogout(Request $request)
    {
        // セッションから全てのキーの値を削除する
        $request->session()->flush();

        return redirect()->route('auths.index');
    }

    // ホームページ表示
    public function showHomePage(Request $request)
    {
        return view('home/index');
    }
}
