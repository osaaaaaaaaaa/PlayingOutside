<?php

namespace App\Http\Controllers;

use App\Models\Attached_Item;
use App\Models\Item;
use App\Models\Mail;
use App\Models\UserMail;
use App\Models\User;
use Barryvdh\Debugbar\Facades\Debugbar;
use Illuminate\Http\Request;
use Illuminate\Pagination\LengthAwarePaginator;
use Illuminate\Support\Facades\Validator;

class MailController extends Controller
{
    // メール一覧表示
    public function index(Request $request)
    {

        $currentPage = $request->page === null ? 1 : $request->page;       // 現在のページ数
        $recordMax = 10;                                                    // １ページに表示する最大件数
        $min = $currentPage > 1 ? ($currentPage - 1) * $recordMax : 0;     // レコードを取得する開始位置

        // メールマスタ取得する(１ページにつき$recordMax件表示する)
        $mails = Mail::offset($min)->limit($recordMax)->get();
        // 最大件数を取得する
        $mailsCnt = Mail::count();
        // 最終的なデータを格納する
        $mailData = [];

        for ($i = 0; $i < count($mails); $i++) {
            // 添付アイテムを取得する
            $attached_items = Attached_Item::selectRaw('items.name AS name, amount')
                ->join('items', 'items.id', '=', 'attached__items.item_id')
                ->where('mail_id', '=', ($i + 1))
                ->get();

            // アイテム情報を結合する
            $itemData = '';
            foreach ($attached_items as $item) {
                $itemData = $itemData . $item['name'] . '×' . $item['amount'] . ' , ';
            }

            // データを格納する
            $array = [
                'id' => $mails[$i]['id'],
                'title' => $mails[$i]['title'],
                'text' => $mails[$i]['text'],
                'item' => $itemData,
                'created_at' => $mails[$i]['created_at'],
                'updated_at' => $mails[$i]['updated_at']
            ];
            $mailData[$i] = $array;
        }

        // 自前の配列をページャーする
        $view_mails = new LengthAwarePaginator($mailData, $mailsCnt, $recordMax, $currentPage,
            array('path' => '/mails/index'));

        return view('mails/index', ['mailData' => $view_mails]);

        // LengthAwarePaginatorについて
        //        [表示するコレクション] = new LengthAwarePaginator(
        //            [表示するコレクション]->forPage([現在のページ番号],[1ページ当たりの表示数]),
        //            [コレクションの大きさ],
        //            [1ページ当たりの表示数],
        //            [現在のページ番号],
        //            [オプション(ここでは"ページの遷移先パス")]
        //        );
    }

    // メール送信ページの表示
    public function create(Request $request)
    {
        // ユーザー情報を取得する
        $userMax = User::count();
        // アイテム情報を取得する
        $items = Item::All();

        return view('mails/create', ['userMax' => $userMax, 'items' => $items, 'normally' => $request['normally']]);
    }

    // メール作成処理
    public function store(Request $request)
    {
        // カスタムバリデーション
        $validator = Validator::make($request->all(), [
            'title' => ['required'],
            'text' => ['required'],
            'type_cnt' => ['required']
        ]);

        if ($validator->fails()) {
            return redirect()->route('mails.create')
                ->withErrors($validator)
                ->withInput();
        }

        //==============
        // 送信処理
        //==============
        $mailID_max = Mail::max('id');  // 最新のメールID

        // メールマスタにレコードを追加
        Mail::create(['title' => $request->title, 'text' => $request->text]);

        // 添付するアイテムをレコードに追加
        if ($request->type_cnt > 0) {
            for ($i = 0; $i < $request->type_cnt; $i++) {
                $id = 'item_id' . ($i + 1);
                $cnt = 'item_cnt' . ($i + 1);
                Attached_Item::create([
                    'mail_id' => ($mailID_max + 1),
                    'item_id' => $request[$id],
                    'amount' => $request[$cnt]
                ]);
            }
        }

        // ユーザーが受け取ったかどうかのテーブルにレコードを追加する
        if ($request->target_id === '0') {
            // 全ユーザー指定の場合
            $users = User::paginate(20);
            foreach ($users as $user) {
                UserMail::create(['user_id' => $user['id'], 'mail_id' => ($mailID_max + 1), 'is_received' => 0]);
            }
            Debugbar::info('全ユーザー指定');
        } else {
            // 特定のユーザー指定の場合
            UserMail::create([
                'user_id' => $request->target_id,
                'mail_id' => ($mailID_max + 1),
                'is_received' => 0
            ]);
            Debugbar::info($request->target_id . 'を指定');
        }

        return redirect()->route('mails.create', ['normally' => 'valid']);
    }
}
