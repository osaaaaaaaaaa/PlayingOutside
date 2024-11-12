<?php

namespace App\Http\Controllers;

use App\Models\DistressSignal;
use App\Models\Guest;
use App\Models\User;
use Illuminate\Http\Request;
use Barryvdh\Debugbar\Facades\Debugbar;
use Illuminate\Pagination\LengthAwarePaginator;

class DistressSignalController extends Controller
{
    public function index(Request $request)
    {
        $currentPage = $request->page === null ? 1 : $request->page;       // 現在のページ数
        $recordMax = 10;                                                    // １ページに表示する最大件数
        $min = $currentPage > 1 ? ($currentPage - 1) * $recordMax : 0;     // レコードを取得する開始位置

        // モデル取得(１ページにつき$recordMax件表示する)
        if (!empty($request->action)) {
            $signals = DistressSignal::where('action', '=', $request->action)
                ->offset($min)->limit($recordMax)->get();
        } else {
            $signals = DistressSignal::offset($min)->limit($recordMax)->get();
        }
        // 最大件数を取得する
        $signalCnt = count($signals);
        // 最終的なデータを格納する
        $signalData = [];

        for ($i = 0; $i < count($signals); $i++) {
            // ホストユーザーを取得する
            $hostUser = User::find($signals[$i]->user_id);

            // ゲストユーザー情報を結合する
            $guests = Guest::where('distress_signal_id', '=', $signals[$i]->id)->get();
            $gustNames = '';
            foreach ($guests as $guest) {
                $user = User::find($guest->user_id);
                $gustNames = $gustNames . $user->name . ' , ';
            }

            // アクションを文字列にする
            switch ($signals[$i]->action) {
                case 0:
                    $action = "挑戦中";
                    break;
                case 1:
                    $action = "ゲームクリア";
                    break;
                default:
                    $action = "NULL";
                    break;
            }

            // データを格納する
            $array = [
                'id' => $signals[$i]->id,
                'host' => $hostUser->name,
                'guests' => $gustNames,
                'stage_id' => $signals[$i]->stage_id,
                'action' => $action,
                'created_at' => $signals[$i]->created_at,
                'updated_at' => $signals[$i]->updated_at
            ];
            $signalData[$i] = $array;
        }

        // 自前の配列をページャーする
        $view_signals = new LengthAwarePaginator($signalData, $signalCnt, $recordMax, $currentPage,
            array('path' => '/distresssignals/index'));

        return view('distresssignals/index', ['distresssignals' => $view_signals]);
    }
}
