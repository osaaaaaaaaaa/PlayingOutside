<?php

namespace App\Http\Controllers;

use App\Models\Achievement;
use App\Models\Item;
use Illuminate\Http\Request;
use Barryvdh\Debugbar\Facades\Debugbar;
use Illuminate\Pagination\LengthAwarePaginator;
use Illuminate\Support\Facades\Validator;

class AchievementController extends Controller
{
    // アチーブメント一覧表示
    public function index(Request $request)
    {
        $currentPage = $request->page === null ? 1 : $request->page;        // 現在のページ数
        $recordMax = 10;                                                    // １ページに表示する最大件数
        $min = $currentPage > 1 ? ($currentPage - 1) * $recordMax : 0;      // レコードを取得する開始位置

        // アチーブメントマスタ取得する(１ページにつき$recordMax件表示する)
        $achievements = Achievement::offset($min)->limit($recordMax)->get();
        // 最大件数を取得する
        $achievementsCnt = Achievement::count();
        // 最終的なデータを格納する
        $achievementData = [];

        for ($i = 0; $i < count($achievements); $i++) {
            // アイテムを取得する
            $item = $achievements[$i]->items;

            // 種類を文字列に出力する
            switch ($achievements[$i]->type) {
                case 1:
                    $type = 'ステージ初回クリア';
                    break;
                case 2:
                    $type = 'トータルスコア';
                    break;
                case 3:
                    $type = 'ポイント報酬';
                    break;
            }

            $strMerge = '';
            switch ($item[0]['type']) {
                case 1:
                    $strMerge = 'アイコン';
                    break;
                case 2:
                    $strMerge = '称号';
                    break;
                case 3:
                    $strMerge = 'お助けアイテム';
                    break;
                case 4:
                    $strMerge = '救難信号解放';
                    break;
                case 5:
                    $strMerge = '救難信号の上限値UP';
                    break;
                case 6:
                    $strMerge = 'ポイント';
                    break;
            }
            $strItem = '[' . $strMerge . '] ' . $item[0]['name'];

            // データを格納する
            $array = [
                'id' => $achievements[$i]['id'],
                'text' => $achievements[$i]['text'],
                'type' => $type,
                'achieved_val' => $achievements[$i]['achieved_val'],
                'item' => $strItem,
                'item_amount' => $achievements[$i]['item_amount']
            ];
            $achievementData[$i] = $array;
        }

        // 自前の配列をページャーする
        $view_achievement = new LengthAwarePaginator($achievementData, $achievementsCnt, $recordMax, $currentPage,
            array('path' => '/achievements/index'));

        return view('achievements/index', ['achievements' => $view_achievement]);
    }

    // アチーブメント作成ページ表示
    public function create(Request $request)
    {
        // アチーブメントの種類
        $type = [
            ['name' => 'ステージ初回クリア'],
            ['name' => 'トータルスコア'],
            ['name' => 'ポイント報酬'],
        ];

        // アイテム情報を取得する
        $items = Item::All();
        for ($i = 0; $i < count($items); $i++) {
            $strMerge = '';
            switch ($items[$i]['type']) {
                case 1:
                    $strMerge = 'アイコン';
                    break;
                case 2:
                    $strMerge = '称号';
                    break;
                case 3:
                    $strMerge = 'お助けアイテム';
                    break;
                case 4:
                    $strMerge = '救難信号解放';
                    break;
                case 5:
                    $strMerge = '救難信号の上限値UP';
                    break;
                case 6:
                    $strMerge = 'ポイント';
                    break;
            }
            $items[$i]['name'] = '[' . $strMerge . '] ' . $items[$i]['name'];
        }

        return view('achievements/create', ['type' => $type, 'items' => $items, 'normally' => $request['normally']]);
    }

    // アチーブメント作成処理
    public function store(Request $request)
    {
        // カスタムバリデーション
        $validator = Validator::make($request->all(), [
            'text' => ['required', 'max:40'],
            'type' => ['required'],
            'achieved_val' => ['required'],
            'item_id' => ['required'],
            'item_amount' => ['required', 'min:1'],
        ]);

        if ($validator->fails()) {
            return redirect()->route('mails.create')
                ->withErrors($validator)
                ->withInput();
        }

        // 挿入処理
        Achievement::create([
            'text' => $request->text,
            'type' => $request->type,
            'achieved_val' => $request->achieved_val,
            'item_id' => $request->item_id,
            'item_amount' => $request->item_amount
        ]);

        return redirect()->route('achievements.create', ['normally' => 'valid']);
    }
}
