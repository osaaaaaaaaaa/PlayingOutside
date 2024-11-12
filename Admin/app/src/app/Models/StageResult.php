<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class StageResult extends Model
{
    // $guardedには更新しないカラムを指定する
    protected $guarded = [
        'id',
    ];
}
